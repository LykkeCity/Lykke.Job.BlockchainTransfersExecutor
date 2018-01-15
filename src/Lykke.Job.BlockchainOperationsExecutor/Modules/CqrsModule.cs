using System.Collections.Generic;
using Autofac;
using Common.Log;
using Inceptum.Cqrs.Configuration;
using Inceptum.Messaging;
using Inceptum.Messaging.Contract;
using Inceptum.Messaging.RabbitMq;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Commands;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core;
using Lykke.Job.BlockchainOperationsExecutor.Settings.JobSettings;
using Lykke.Job.BlockchainOperationsExecutor.Workflow;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Sagas;
using Lykke.Messaging;

namespace Lykke.Job.BlockchainOperationsExecutor.Modules
{
    public class CqrsModule : Module
    {
        private static readonly string Self = BlockchainOperationsExecutorBoundedContext.Name;

        private readonly CqrsSettings _settings;
        private readonly ChaosSettings _chaosSettings;
        private readonly ILog _log;

        public CqrsModule(CqrsSettings settings, ChaosSettings chaosSettings, ILog log)
        {
            _settings = settings;
            _chaosSettings = chaosSettings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            if (_chaosSettings != null)
            {
                ChaosKitty.StateOfChaos = _chaosSettings.StateOfChaos;
            }

            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>().SingleInstance();

            var rabbitMqSettings = new RabbitMQ.Client.ConnectionFactory
            {
                Uri = _settings.RabbitConnectionString
            };
            var messagingEngine = new MessagingEngine(_log,
                new TransportResolver(new Dictionary<string, TransportInfo>
                {
                    {
                        "RabbitMq",
                        new TransportInfo(rabbitMqSettings.Endpoint.ToString(), rabbitMqSettings.UserName,
                            rabbitMqSettings.Password, "None", "RabbitMq")
                    }
                }),
                new RabbitMqTransportFactory());

            builder.Register(c => new RetryDelayProvider(
                    _settings.SourceAddressLockingRetryDelay,
                    _settings.WaitForTransactionRetryDelay))
                .AsSelf();

            // Sagas
            builder.RegisterType<OperationExecutionSaga>();

            // Command handlers
            builder.RegisterType<StartOperationCommandsHandler>();
            builder.RegisterType<BuildTransactionCommandsHandler>();
            builder.RegisterType<SignTransactionCommandsHandler>();
            builder.RegisterType<BroadcastTransactionCommandsHandler>();
            builder.RegisterType<WaitForTransactionEndingCommandsHandler>();
            builder.RegisterType<ReleaseSourceAddressLockCommandsHandler>();

            builder.Register(ctx => CreateEngine(ctx, messagingEngine))
                .As<ICqrsEngine>()
                .SingleInstance()
                .AutoActivate();
        }

        private CqrsEngine CreateEngine(IComponentContext ctx, IMessagingEngine messagingEngine)
        {
            var defaultRetryDelay = (long)_settings.RetryDelay.TotalMilliseconds;

            return new CqrsEngine(
                _log,
                ctx.Resolve<IDependencyResolver>(),
                messagingEngine,
                new DefaultEndpointProvider(),
                true,
                Register.DefaultEndpointResolver(new RabbitMqConventionEndpointResolver("RabbitMq", "protobuf")),

                Register.BoundedContext(Self)
                    .FailedCommandRetryDelay(defaultRetryDelay)

                    .ListeningCommands(typeof(StartOperationCommand))
                    .On("start")
                    .WithCommandsHandler<StartOperationCommandsHandler>()
                    .PublishingEvents(typeof(OperationStartRequestedEvent))
                    .With("start-requested")

                    .ListeningCommands(typeof(BuildTransactionCommand))
                    .On("build-tx")
                    .WithCommandsHandler<BuildTransactionCommandsHandler>()
                    .PublishingEvents(typeof(TransactionBuiltEvent))
                    .With("tx-built")

                    .ListeningCommands(typeof(SignTransactionCommand))
                    .On("sign-tx")
                    .WithCommandsHandler<SignTransactionCommandsHandler>()
                    .PublishingEvents(typeof(TransactionSignedEvent))
                    .With("tx-signed")

                    .ListeningCommands(typeof(BroadcastTransactionCommand))
                    .On("broadcast-tx")
                    .WithCommandsHandler<BroadcastTransactionCommandsHandler>()
                    .PublishingEvents(typeof(TransactionBroadcastedEvent))
                    .With("tx-broadcasted")

                    .ListeningCommands(typeof(WaitForTransactionEndingCommand))
                    .On("wait-for-tx-finish")
                    .WithCommandsHandler<WaitForTransactionEndingCommandsHandler>()
                    .PublishingEvents(
                        typeof(OperationCompletedEvent),
                        typeof(OperationFailedEvent))
                    .With("finished")
                    
                    .ListeningCommands(typeof(ReleaseSourceAddressLockCommand))
                    .On("release-source-address")
                    .WithCommandsHandler<ReleaseSourceAddressLockCommandsHandler>()
                    .PublishingEvents(typeof(SourceAddressLockReleasedEvent))
                    .With("source-address-released")

                    .ProcessingOptions("start").MultiThreaded(4).QueueCapacity(1024)
                    .ProcessingOptions("build-tx").MultiThreaded(4).QueueCapacity(1024)
                    .ProcessingOptions("sign-tx").MultiThreaded(4).QueueCapacity(1024)
                    .ProcessingOptions("broadcast-tx").MultiThreaded(4).QueueCapacity(1024)
                    .ProcessingOptions("wait-for-tx-finish").MultiThreaded(4).QueueCapacity(1024)
                    .ProcessingOptions("release-source-address").MultiThreaded(4).QueueCapacity(1024),

                Register.Saga<OperationExecutionSaga>($"{Self}.operation-execution-saga")
                    .ListeningEvents(typeof(OperationStartRequestedEvent))
                    .From(Self)
                    .On("start-requested")
                    .PublishingCommands(typeof(BuildTransactionCommand))
                    .To(Self)
                    .With("build-tx")

                    .ListeningEvents(typeof(TransactionBuiltEvent))
                    .From(Self)
                    .On("tx-built")
                    .PublishingCommands(typeof(SignTransactionCommand))
                    .To(Self)
                    .With("sign-tx")

                    .ListeningEvents(typeof(TransactionSignedEvent))
                    .From(Self)
                    .On("tx-signed")
                    .PublishingCommands(typeof(BroadcastTransactionCommand))
                    .To(Self)
                    .With("broadcast-tx")

                    .ListeningEvents(typeof(TransactionBroadcastedEvent))
                    .From(Self)
                    .On("tx-broadcasted")
                    .PublishingCommands(typeof(WaitForTransactionEndingCommand))
                    .To(Self)
                    .With("wait-for-tx-finish")

                    .ListeningEvents(
                        typeof(OperationCompletedEvent),
                        typeof(OperationFailedEvent))
                    .From(Self)
                    .On("finished")
                    .PublishingCommands(typeof(ReleaseSourceAddressLockCommand))
                    .To(Self)
                    .With("release-source-address")

                    .ListeningEvents(typeof(SourceAddressLockReleasedEvent))
                    .From(Self)
                    .On("source-address-released"));
        }
    }
}
