using System.Collections.Generic;
using Autofac;
using Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Job.BlockchainOperationsExecutor.Contract;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Commands;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;
using Lykke.Job.BlockchainOperationsExecutor.Services.Transitions;
using Lykke.Job.BlockchainOperationsExecutor.Services.Transitions.Interfaces;
using Lykke.Job.BlockchainOperationsExecutor.Settings.JobSettings;
using Lykke.Job.BlockchainOperationsExecutor.Workflow;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Sagas;
using Lykke.Messaging;
using Lykke.Messaging.Contract;
using Lykke.Messaging.RabbitMq;

namespace Lykke.Job.BlockchainOperationsExecutor.Modules
{
    public class CqrsModule : Module
    {
        private static readonly string Self = BlockchainOperationsExecutorBoundedContext.Name;

        private readonly CqrsSettings _settings;
        private readonly ILog _log;
        private readonly string _rabbitMqVirtualHost;

        public CqrsModule(CqrsSettings settings, ILog log, string rabbitMqVirtualHost = null)
        {
            _settings = settings;
            _log = log;
            _rabbitMqVirtualHost = rabbitMqVirtualHost;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>().SingleInstance();

            var rabbitMqSettings = new RabbitMQ.Client.ConnectionFactory
            {
                Uri = _settings.RabbitConnectionString
            };

            var rabbitMqEndpoint = _rabbitMqVirtualHost == null
                ? rabbitMqSettings.Endpoint.ToString()
                : $"{rabbitMqSettings.Endpoint}/{_rabbitMqVirtualHost}";

            var messagingEngine = new MessagingEngine(_log,
                new TransportResolver(new Dictionary<string, TransportInfo>
                {
                    {
                        "RabbitMq",
                        new TransportInfo(rabbitMqEndpoint, rabbitMqSettings.UserName,
                            rabbitMqSettings.Password, "None", "RabbitMq")
                    }
                }),                
                new RabbitMqTransportFactory());

            builder.Register(c => new RetryDelayProvider(
                    _settings.SourceAddressLockingRetryDelay,
                    _settings.WaitForTransactionRetryDelay,
                    _settings.NotEnoughBalanceRetryDelay))
                .AsSelf();

            builder.RegisterInstance(TransitionCheckerFactory.BuildTransitionsForService())
                .As<ITransitionChecker<TransactionExecutionState>>();

            // Sagas
            builder.RegisterType<TransactionExecutionSaga>();

            // Command handlers
            builder.RegisterType<StartOperationExecutionCommandsHandler>();
            builder.RegisterType<BuildTransactionCommandsHandler>();
            builder.RegisterType<SignTransactionCommandsHandler>();
            builder.RegisterType<BroadcastTransactionCommandsHandler>();
            builder.RegisterType<WaitForTransactionEndingCommandsHandler>();
            builder.RegisterType<ReleaseSourceAddressLockCommandsHandler>();
            builder.RegisterType<ClearTransactionCommandsHandler>();

            builder.Register(ctx => CreateEngine(ctx, messagingEngine))
                .As<ICqrsEngine>()
                .SingleInstance()
                .AutoActivate();
        }

        private CqrsEngine CreateEngine(IComponentContext ctx, IMessagingEngine messagingEngine)
        {
            var defaultRetryDelay = (long)_settings.RetryDelay.TotalMilliseconds;

            const string defaultPipeline = "commands";
            const string defaultRoute = "self";

            return new CqrsEngine(
                _log,
                ctx.Resolve<IDependencyResolver>(),
                messagingEngine,
                new DefaultEndpointProvider(),
                true,
                Register.DefaultEndpointResolver(new RabbitMqConventionEndpointResolver(
                    "RabbitMq",
                    "messagepack",
                    environment: "lykke")),

                Register.BoundedContext(Self)
                    .FailedCommandRetryDelay(defaultRetryDelay)

                    .ListeningCommands(typeof(StartOperationExecutionCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<StartOperationExecutionCommandsHandler>()
                    .PublishingEvents(typeof(OperationExecutionStartedEvent))
                    .With(defaultPipeline)

                    .ListeningCommands(typeof(BuildTransactionCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<BuildTransactionCommandsHandler>()
                    .PublishingEvents(
                        typeof(TransactionBuiltEvent),
                        typeof(TransactionBuildingRejectedEvent),
                        typeof(TransactionBuildingFailedEvent))
                    .With(defaultPipeline)

                    .ListeningCommands(typeof(SignTransactionCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<SignTransactionCommandsHandler>()
                    .PublishingEvents(typeof(TransactionSignedEvent))
                    .With(defaultPipeline)

                    .ListeningCommands(typeof(BroadcastTransactionCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<BroadcastTransactionCommandsHandler>()
                    .PublishingEvents(
                        typeof(TransactionBroadcastedEvent), 
                        typeof(TransactionBroadcastingFailedEvent),
                        typeof(TransactionReBuildingIsRequestedOnBroadcastingEvent))
                    .With(defaultPipeline)

                    .ListeningCommands(typeof(ReleaseSourceAddressLockCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<ReleaseSourceAddressLockCommandsHandler>()
                    .PublishingEvents(typeof(SourceAddressLockReleasedEvent), 
                        typeof(TransactionReBuildingIsRequestedEvent))
                    .With(defaultPipeline)

                    .ListeningCommands(typeof(WaitForTransactionEndingCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<WaitForTransactionEndingCommandsHandler>()
                    .PublishingEvents(
                        typeof(OperationExecutionCompletedEvent),
                        typeof(OperationExecutionFailedEvent),
                        typeof(TransactionReBuildingIsRequestedEvent))
                    .With(defaultPipeline)
                    
                    .ListeningCommands(typeof(ClearTransactionCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<ClearTransactionCommandsHandler>()
                    .PublishingEvents(typeof(TransactionClearedEvent))
                    .With(defaultPipeline)

                    .ProcessingOptions(defaultRoute).MultiThreaded(8).QueueCapacity(1024),

                Register.Saga<TransactionExecutionSaga>($"{Self}.saga")
                    .ListeningEvents(typeof(OperationExecutionStartedEvent), 
                        typeof(TransactionReBuildingIsRequestedEvent))
                    .From(Self)
                    .On(defaultRoute)
                    .PublishingCommands(typeof(BuildTransactionCommand))
                    .To(Self)
                    .With(defaultPipeline)

                    .ListeningEvents(typeof(TransactionBuiltEvent))
                    .From(Self)
                    .On(defaultRoute)
                    .PublishingCommands(typeof(SignTransactionCommand))
                    .To(Self)
                    .With(defaultPipeline)

                    .ListeningEvents(typeof(TransactionSignedEvent))
                    .From(Self)
                    .On(defaultRoute)
                    .PublishingCommands(typeof(BroadcastTransactionCommand))
                    .To(Self)
                    .With(defaultPipeline)

                    .ListeningEvents(
                        typeof(TransactionBroadcastedEvent),
                        typeof(TransactionBroadcastingFailedEvent),
                        typeof(TransactionBuildingRejectedEvent),
                        typeof(TransactionBuildingFailedEvent),
                        typeof(TransactionReBuildingIsRequestedOnBroadcastingEvent))
                    .From(Self)
                    .On(defaultRoute)
                    .PublishingCommands(typeof(ReleaseSourceAddressLockCommand))
                    .To(Self)
                    .With(defaultPipeline)

                    .ListeningEvents(typeof(SourceAddressLockReleasedEvent))
                    .From(Self)
                    .On(defaultRoute)
                    .PublishingCommands(typeof(WaitForTransactionEndingCommand))
                    .To(Self)
                    .With(defaultPipeline)

                    .ListeningEvents(
                        typeof(OperationExecutionCompletedEvent),
                        typeof(OperationExecutionFailedEvent))
                    .From(Self)
                    .On(defaultRoute)
                    .PublishingCommands(typeof(ClearTransactionCommand))
                    .To(Self)
                    .With(defaultPipeline)

                    .ListeningEvents(typeof(TransactionClearedEvent))
                    .From(Self)
                    .On(defaultRoute));
        }
    }
}
