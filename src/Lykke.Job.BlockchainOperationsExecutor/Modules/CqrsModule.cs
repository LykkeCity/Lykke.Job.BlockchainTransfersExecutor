using System.Collections.Generic;
using Autofac;
using Autofac.Features.ResolveAnything;
using Lykke.Common.Log;
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
using Lykke.Messaging.RabbitMq;
using Lykke.Messaging.Serialization;
using OperationExecutionStartedEvent = Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.OperationExecution.OperationExecutionStartedEvent;

namespace Lykke.Job.BlockchainOperationsExecutor.Modules
{
    public class CqrsModule : Module
    {
        private static readonly string OperationsExecutor = BlockchainOperationsExecutorBoundedContext.Name;
        public static readonly string TransactionExecutor = "bcn-integration.transactions-executor";

        private readonly CqrsSettings _settings;
        private readonly string _rabbitMqVirtualHost;

        public CqrsModule(CqrsSettings settings, string rabbitMqVirtualHost = null)
        {
            _settings = settings;
            _rabbitMqVirtualHost = rabbitMqVirtualHost;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>().SingleInstance();

            builder.Register(c => new RetryDelayProvider(
                    _settings.SourceAddressLockingRetryDelay,
                    _settings.WaitForTransactionRetryDelay,
                    _settings.NotEnoughBalanceRetryDelay,
                    _settings.RebuildingConfirmationCheckRetryDelay))
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

            builder.Register(CreateEngine)
                .As<ICqrsEngine>()
                .SingleInstance()
                .AutoActivate();
        }

        private CqrsEngine CreateEngine(IComponentContext ctx)
        {
            var rabbitMqSettings = new RabbitMQ.Client.ConnectionFactory
            {
                Uri = _settings.RabbitConnectionString
            };

            var rabbitMqEndpoint = _rabbitMqVirtualHost == null
                ? rabbitMqSettings.Endpoint.ToString()
                : $"{rabbitMqSettings.Endpoint}/{_rabbitMqVirtualHost}";

            var logFactory = ctx.Resolve<ILogFactory>();

            var messagingEngine = new MessagingEngine(
                logFactory,
                new TransportResolver(new Dictionary<string, TransportInfo>
                {
                    {
                        "RabbitMq",
                        new TransportInfo(rabbitMqEndpoint, rabbitMqSettings.UserName,
                            rabbitMqSettings.Password, "None", "RabbitMq")
                    }
                }),
                new RabbitMqTransportFactory(logFactory));

            var defaultRetryDelay = (long)_settings.RetryDelay.TotalMilliseconds;

            const string commandsPipeline = "commands";
            const string defaultRoute = "self";

            return new CqrsEngine
            (
                logFactory,
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
                    .With(commandsPipeline)

                    .ListeningCommands(typeof(StartOneToManyOutputsExecutionCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler(typeof(StartOneToManyOperationExecutionCommandsHandler))
                    .PublishingEvents(typeof(OperationExecutionStartedEvent))
                    .With(commandsPipeline)

                    .ListeningCommands(typeof(GenerateActiveTransactionIdCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<GenerateActiveTransactionIdCommandsHandler>()
                    .PublishingEvents(typeof(ActiveTransactionIdGeneratedEvent), typeof(TransactionReBuildingRejectedEvent))
                    .With(commandsPipeline)

                    .ListeningCommands(typeof(ClearActiveTransactionCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<ClearActiveTransactionCommandsHandler>()
                    .PublishingEvents(typeof(ActiveTransactionClearedEvent))
                    .With(commandsPipeline)

                    .ListeningCommands(typeof(NotifyOperationExecutionCompletedCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<NotifyOperationExecutionCompletedCommandsHandler>()
                    .PublishingEvents(
                        typeof(OperationExecutionCompletedEvent),
                        typeof(OneToManyOperationExecutionCompletedEvent))
                    .With(commandsPipeline)

                    .ListeningCommands(typeof(NotifyOperationExecutionFailedCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<NotifyOperationExecutionFailedCommandsHandler>()
                    .PublishingEvents(typeof(OperationExecutionFailedEvent))
                    .With(commandsPipeline)

                    .ProcessingOptions(defaultRoute).MultiThreaded(8).QueueCapacity(1024),

                Register.BoundedContext(TransactionExecutor)
                    .FailedCommandRetryDelay(defaultRetryDelay)

                    .ListeningCommands(typeof(StartTransactionExecutionCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<StartTransactionExecutionCommandHandler>()
                    .PublishingEvents(typeof(TransactionExecutionStartedEvent))
                    .With(commandsPipeline)

                    .ListeningCommands(typeof(LockSourceAddressCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<LockSourceAddressCommandsHandler>()
                    .PublishingEvents(typeof(SourceAddressLockedEvent))
                    .With(commandsPipeline)

                    .ListeningCommands(typeof(LockSourceAndTargetAddressesCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<LockSourceAndTargetAddressesCommandsHandler>()
                    .PublishingEvents(typeof(SourceAndTargetAddressesLockedEvent))
                    .With(commandsPipeline)
                    
                    .ListeningCommands(typeof(BuildTransactionCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<BuildTransactionCommandsHandler>()
                    .PublishingEvents(
                        typeof(TransactionBuiltEvent),
                        typeof(TransactionBuildingRejectedEvent),
                        typeof(TransactionExecutionFailedEvent))
                    .With(commandsPipeline)

                    .ListeningCommands(typeof(SignTransactionCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<SignTransactionCommandsHandler>()
                    .PublishingEvents(typeof(TransactionSignedEvent))
                    .With(commandsPipeline)

                    .ListeningCommands(typeof(BroadcastTransactionCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<BroadcastTransactionCommandsHandler>()
                    .PublishingEvents(
                        typeof(TransactionBroadcastedEvent),
                        typeof(TransactionExecutionFailedEvent),
                        typeof(TransactionExecutionRepeatRequestedEvent))
                    .With(commandsPipeline)

                    .ListeningCommands(typeof(ReleaseSourceAddressLockCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<ReleaseSourceAddressLockCommandsHandler>()
                    .PublishingEvents(typeof(SourceAddressLockReleasedEvent))
                    .With(commandsPipeline)

                    .ListeningCommands(typeof(ReleaseSourceAndTargetAddressLocksCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<ReleaseSourceAndTargetAddressLocksCommandsHandler>()
                    .PublishingEvents(typeof(SourceAddressLockReleasedEvent))
                    .With(commandsPipeline)
                    
                    .ListeningCommands(typeof(WaitForTransactionEndingCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<WaitForTransactionEndingCommandsHandler>()
                    .PublishingEvents(
                        typeof(TransactionExecutionCompletedEvent),
                        typeof(TransactionExecutionFailedEvent),
                        typeof(TransactionExecutionRepeatRequestedEvent))
                    .With(commandsPipeline)

                    .ListeningCommands(typeof(ClearBroadcastedTransactionCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<ClearBroadcastedTransactionCommandsHandler>()
                    .PublishingEvents(typeof(BroadcastedTransactionClearedEvent))
                    .With(commandsPipeline)

                    .ProcessingOptions(defaultRoute).MultiThreaded(8).QueueCapacity(1024),

                Register.Saga<OperationExecutionSaga>($"{OperationsExecutor}.saga")
                    .ListeningEvents(typeof(OperationExecutionStartedEvent))
                    .From(OperationsExecutor)
                    .On(defaultRoute)
                    .PublishingCommands(typeof(GenerateActiveTransactionIdCommand))
                    .To(OperationsExecutor)
                    .With(commandsPipeline)

                    .ListeningEvents(typeof(ActiveTransactionIdGeneratedEvent))
                    .From(OperationsExecutor)
                    .On(defaultRoute)
                    .PublishingCommands(typeof(StartTransactionExecutionCommand))
                    .To(TransactionExecutor)
                    .With(commandsPipeline)

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
                    .With(commandsPipeline)

                    .ListeningEvents(typeof(TransactionReBuildingRejectedEvent))
                    .From(OperationsExecutor)
                    .On(defaultRoute)
                    .PublishingCommands(typeof(NotifyOperationExecutionFailedCommand))
                    .To(OperationsExecutor)
                    .With(commandsPipeline)

                    .ListeningEvents(typeof(ActiveTransactionClearedEvent))
                    .From(OperationsExecutor)
                    .On(defaultRoute)
                    .PublishingCommands(typeof(GenerateActiveTransactionIdCommand))
                    .To(OperationsExecutor)
                    .With(commandsPipeline)

                    .ListeningEvents
                    (
                        typeof(OperationExecutionCompletedEvent),
                        typeof(OneToManyOperationExecutionCompletedEvent),
                        typeof(OperationExecutionFailedEvent)
                    )
                    .From(OperationsExecutor)
                    .On(defaultRoute)
                
                    .ProcessingOptions(defaultRoute).MultiThreaded(8).QueueCapacity(1024),

                Register.Saga<TransactionExecutionSaga>($"{TransactionExecutor}.saga")
                    .ListeningEvents(typeof(TransactionExecutionStartedEvent))
                    .From(TransactionExecutor)
                    .On(defaultRoute)
                    .PublishingCommands
                    (
                        typeof(LockSourceAddressCommand),
                        typeof(LockSourceAndTargetAddressesCommand)
                    )
                    .To(TransactionExecutor)
                    .With(commandsPipeline)

                    .ListeningEvents
                    (
                        typeof(SourceAddressLockedEvent),
                        typeof(SourceAndTargetAddressesLockedEvent)
                    )
                    .From(TransactionExecutor)
                    .On(defaultRoute)
                    .PublishingCommands(typeof(BuildTransactionCommand))
                    .To(TransactionExecutor)
                    .With(commandsPipeline)

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
                        typeof(ReleaseSourceAddressLockCommand),
                        typeof(ReleaseSourceAndTargetAddressLocksCommand)
                    )
                    .To(TransactionExecutor)
                    .With(commandsPipeline)

                    .ListeningEvents(typeof(TransactionSignedEvent))
                    .From(TransactionExecutor)
                    .On(defaultRoute)
                    .PublishingCommands(typeof(BroadcastTransactionCommand))
                    .To(TransactionExecutor)
                    .With(commandsPipeline)

                    .ListeningEvents
                    (
                        typeof(TransactionBroadcastedEvent),
                        typeof(TransactionExecutionFailedEvent),
                        typeof(TransactionExecutionRepeatRequestedEvent)
                    )
                    .From(TransactionExecutor)
                    .On(defaultRoute)
                    .PublishingCommands
                    (
                        typeof(ReleaseSourceAddressLockCommand),
                        typeof(ReleaseSourceAndTargetAddressLocksCommand),
                        typeof(WaitForTransactionEndingCommand)
                    )
                    .To(TransactionExecutor)
                    .With(commandsPipeline)

                    .ListeningEvents(typeof(SourceAddressLockReleasedEvent))
                    .From(TransactionExecutor)
                    .On(defaultRoute)
                    .PublishingCommands
                    (
                        typeof(WaitForTransactionEndingCommand),
                        typeof(ClearBroadcastedTransactionCommand)
                    )
                    .To(TransactionExecutor)
                    .With(commandsPipeline)

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
                        typeof(ClearBroadcastedTransactionCommand),
                        typeof(ReleaseSourceAndTargetAddressLocksCommand)
                    )
                    .To(TransactionExecutor)
                    .With(commandsPipeline)
                    
                    .ListeningEvents
                    (
                        typeof(SourceAndTargetAddressLocksReleasedEvent)
                    )
                    .From(TransactionExecutor)
                    .On(defaultRoute)
                    .PublishingCommands(typeof(ClearBroadcastedTransactionCommand))
                    .To(TransactionExecutor)
                    .With(commandsPipeline)

                    .ListeningEvents(typeof(BroadcastedTransactionClearedEvent))
                    .From(TransactionExecutor)
                    .On(defaultRoute)

                    .ProcessingOptions(defaultRoute).MultiThreaded(8).QueueCapacity(1024)
            );
        }
    }
}
