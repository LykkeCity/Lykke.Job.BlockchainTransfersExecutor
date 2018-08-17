using System.Collections.Generic;
using Autofac;
using Autofac.Features.ResolveAnything;
using Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Job.BlockchainOperationsExecutor.Contract;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Commands;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.OperationExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Settings.JobSettings;
using Lykke.Job.BlockchainOperationsExecutor.StateMachine;
using Lykke.Job.BlockchainOperationsExecutor.Workflow;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers.OperationExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers.TransactionExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.OperationExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.TransactionExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.OperationExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Sagas;
using Lykke.Messaging;
using Lykke.Messaging.Contract;
using Lykke.Messaging.RabbitMq;
using Lykke.Messaging.Serialization;

namespace Lykke.Job.BlockchainOperationsExecutor.Modules
{
    public class CqrsModule : Module
    {
        private static readonly string OperationsExecutor = BlockchainOperationsExecutorBoundedContext.Name;
        public static readonly string TransactionExecutor = "bcn-integration.transactions-executor";

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

            builder.RegisterInstance(TransitionExecutionStateSwitcherBuilder.Build())
                .As<IStateSwitcher<TransactionExecutionAggregate>>();

            builder.RegisterInstance(OperationExecutionStateSwitcherBuilder.Build())
                .As<IStateSwitcher<OperationExecutionAggregate>>();

            // Sagas
            builder.RegisterType<TransactionExecutionSaga>();
            builder.RegisterType<OperationExecutionSaga>();

            // Command handlers
            builder.RegisterSource(new AnyConcreteTypeNotAlreadyRegisteredSource(t =>
                t.Namespace == typeof(StartOperationExecutionCommandsHandler).Namespace ||
                t.Namespace == typeof(StartTransactionExecutionCommandHandler).Namespace));

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

            return new CqrsEngine
            (
                _log,
                ctx.Resolve<IDependencyResolver>(),
                messagingEngine,
                new DefaultEndpointProvider(),
                true,
                Register.DefaultEndpointResolver
                (
                    new RabbitMqConventionEndpointResolver
                    (
                        "RabbitMq",
                        SerializationFormat.MessagePack,
                        environment: "lykke"
                    )
                ),

                Register.BoundedContext(OperationsExecutor)
                    .FailedCommandRetryDelay(defaultRetryDelay)

                    .ListeningCommands(typeof(StartOperationExecutionCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<StartOperationExecutionCommandsHandler>()
                    .PublishingEvents(typeof(OperationExecutionStartedEvent))
                    .With(defaultPipeline)

                    .ListeningCommands(typeof(GenerateActiveTransactionIdCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<GenerateActiveTransactionIdCommandsHandler>()
                    .PublishingEvents(typeof(ActiveTransactionIdGeneratedEvent))
                    .With(defaultPipeline)

                    .ListeningCommands(typeof(ClearActiveTransactionCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<ClearActiveTransactionCommandsHandler>()
                    .PublishingEvents(typeof(ActiveTransactionClearedEvent))
                    .With(defaultPipeline)

                    .ListeningCommands(typeof(NotifyOperationExecutionCompletedCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<NotifyOperationExecutionCompletedCommandsHandler>()
                    .PublishingEvents(typeof(OperationExecutionCompletedEvent))
                    .With(defaultRoute)

                    .ListeningCommands(typeof(NotifyOperationExecutionFailedCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<NotifyOperationExecutionFailedCommandsHandler>()
                    .PublishingEvents(typeof(OperationExecutionFailedEvent))
                    .With(defaultRoute)

                    .ProcessingOptions(defaultRoute).MultiThreaded(8).QueueCapacity(1024),

                Register.BoundedContext(TransactionExecutor)
                    .FailedCommandRetryDelay(defaultRetryDelay)

                    .ListeningCommands(typeof(StartTransactionExecutionCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<StartTransactionExecutionCommandHandler>()
                    .PublishingEvents(typeof(TransactionExecutionStartedEvent))
                    .With(defaultRoute)

                    .ListeningCommands(typeof(LockSourceAddressCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<LockSourceAddressCommandsHandler>()
                    .PublishingEvents(typeof(SourceAddressLockedEvent))
                    .With(defaultRoute)

                    .ListeningCommands(typeof(BuildTransactionCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<BuildTransactionCommandsHandler>()
                    .PublishingEvents(
                        typeof(TransactionBuiltEvent),
                        typeof(TransactionBuildingRejectedEvent),
                        typeof(TransactionExecutionFailedEvent))
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
                        typeof(TransactionExecutionFailedEvent),
                        typeof(TransactionExecutionRepeatRequestedEvent))
                    .With(defaultPipeline)

                    .ListeningCommands(typeof(ReleaseSourceAddressLockCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<ReleaseSourceAddressLockCommandsHandler>()
                    .PublishingEvents(typeof(SourceAddressLockReleasedEvent))
                    .With(defaultPipeline)

                    .ListeningCommands(typeof(WaitForTransactionEndingCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<WaitForTransactionEndingCommandsHandler>()
                    .PublishingEvents(
                        typeof(TransactionExecutionCompletedEvent),
                        typeof(TransactionExecutionFailedEvent),
                        typeof(TransactionExecutionRepeatRequestedEvent))
                    .With(defaultPipeline)

                    .ListeningCommands(typeof(ClearBroadcastedTransactionCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<ClearBroadcastedTransactionCommandsHandler>()
                    .PublishingEvents(typeof(BroadcastedTransactionClearedEvent))
                    .With(defaultPipeline)

                    .ProcessingOptions(defaultRoute).MultiThreaded(8).QueueCapacity(1024),

                Register.Saga<TransactionExecutionSaga>($"{OperationsExecutor}.saga")
                    .ListeningEvents(typeof(OperationExecutionStartedEvent))
                    .From(OperationsExecutor)
                    .On(defaultRoute)
                    .PublishingCommands(typeof(GenerateActiveTransactionIdCommand))
                    .To(OperationsExecutor)
                    .With(defaultPipeline)

                    .ListeningEvents(typeof(ActiveTransactionIdGeneratedEvent))
                    .From(OperationsExecutor)
                    .On(defaultRoute)
                    .PublishingCommands(typeof(StartTransactionExecutionCommand))
                    .To(TransactionExecutor)
                    .With(defaultPipeline)

                    .ListeningEvents(typeof(TransactionExecutionStartedEvent))
                    .From(TransactionExecutor)
                    .On(defaultRoute)

                    .ListeningEvents
                    (
                        typeof(TransactionExecutionCompletedEvent),
                        typeof(TransactionExecutionFailedEvent),
                        typeof(TransactionExecutionRepeatRequestedEvent)
                    )
                    .From(TransactionExecutor)
                    .On(defaultRoute)
                    .PublishingCommands
                    (
                        typeof(NotifyOperationExecutionCompletedCommand),
                        typeof(NotifyOperationExecutionFailedCommand),
                        typeof(ClearActiveTransactionCommand)
                    )
                    .To(OperationsExecutor)
                    .With(defaultRoute)

                    .ListeningEvents(typeof(ActiveTransactionClearedEvent))
                    .From(OperationsExecutor)
                    .On(defaultRoute)
                    .PublishingCommands(typeof(GenerateActiveTransactionIdCommand))
                    .To(OperationsExecutor)
                    .With(defaultRoute)

                    .ListeningEvents
                    (
                        typeof(OperationExecutionCompletedEvent),
                        typeof(OperationExecutionFailedEvent)
                    )
                    .From(OperationsExecutor)
                    .On(defaultRoute),

                Register.Saga<TransactionExecutionSaga>($"{TransactionExecutor}.saga")
                    .ListeningEvents(typeof(TransactionExecutionStartedEvent))
                    .From(TransactionExecutor)
                    .On(defaultRoute)
                    .PublishingCommands(typeof(LockSourceAddressCommand))
                    .To(TransactionExecutor)
                    .With(defaultRoute)

                    .ListeningEvents(typeof(SourceAddressLockedEvent))
                    .From(TransactionExecutor)
                    .On(defaultRoute)
                    .PublishingCommands(typeof(BuildTransactionCommand))
                    .To(TransactionExecutor)
                    .With(defaultRoute)

                    .ListeningEvents
                    (
                        typeof(TransactionBuiltEvent),
                        typeof(TransactionExecutionFailedEvent),
                        typeof(TransactionBuildingRejectedEvent)
                    )
                    .From(TransactionExecutor)
                    .On(defaultRoute)
                    .PublishingCommands
                    (
                        typeof(SignTransactionCommand),
                        typeof(ReleaseSourceAddressLockCommand)
                    )
                    .To(TransactionExecutor)
                    .With(defaultPipeline)

                    .ListeningEvents(typeof(TransactionSignedEvent))
                    .From(TransactionExecutor)
                    .On(defaultRoute)
                    .PublishingCommands(typeof(BroadcastTransactionCommand))
                    .To(TransactionExecutor)
                    .With(defaultPipeline)

                    .ListeningEvents
                    (
                        typeof(TransactionBroadcastedEvent),
                        typeof(TransactionExecutionFailedEvent),
                        typeof(TransactionExecutionRepeatRequestedEvent)
                    )
                    .From(TransactionExecutor)
                    .On(defaultRoute)
                    .PublishingCommands(typeof(ReleaseSourceAddressLockCommand))
                    .To(TransactionExecutor)
                    .With(defaultPipeline)

                    .ListeningEvents(typeof(SourceAddressLockReleasedEvent))
                    .From(TransactionExecutor)
                    .On(defaultRoute)
                    .PublishingCommands
                    (
                        typeof(WaitForTransactionEndingCommand),
                        typeof(ClearBroadcastedTransactionCommand)
                    )
                    .To(TransactionExecutor)
                    .With(defaultPipeline)

                    .ListeningEvents
                    (
                        typeof(TransactionExecutionCompletedEvent),
                        typeof(TransactionExecutionFailedEvent),
                        typeof(TransactionExecutionRepeatRequestedEvent)
                    )
                    .From(TransactionExecutor)
                    .On(defaultRoute)
                    .PublishingCommands(typeof(ClearBroadcastedTransactionCommand))
                    .To(TransactionExecutor)
                    .With(defaultPipeline)

                    .ListeningEvents(typeof(BroadcastedTransactionClearedEvent))
                    .From(TransactionExecutor)
                    .On(defaultRoute)
            );
        }
    }
}
