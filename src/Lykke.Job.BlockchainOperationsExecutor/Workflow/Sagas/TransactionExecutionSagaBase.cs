using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains;
using Lykke.Job.BlockchainOperationsExecutor.Mappers;
using Lykke.Job.BlockchainOperationsExecutor.Modules;
using Lykke.Job.BlockchainOperationsExecutor.StateMachine;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.TransactionExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Sagas
{
    public abstract class TransactionExecutionSagaBase
    {
        private readonly IBlockchainSettingsProvider _blockchainSettingsProvider;
        private readonly IChaosKitty _chaosKitty;
        private readonly ITransactionExecutionsRepository _repository;
        private readonly IStateSwitcher<TransactionExecutionAggregate> _stateSwitcher;
        
        protected static string Self => CqrsModule.TransactionExecutor;

        protected TransactionExecutionSagaBase(
            IBlockchainSettingsProvider blockchainSettingsProvider,
            IChaosKitty chaosKitty,
            ITransactionExecutionsRepository repository, 
            IStateSwitcher<TransactionExecutionAggregate> stateSwitcher)
        {
            _blockchainSettingsProvider = blockchainSettingsProvider;
            _chaosKitty = chaosKitty;            
            _repository = repository;
            _stateSwitcher = stateSwitcher;
        }

        


        protected bool ExclusiveWithdrawalsLockingRequired(
            TransactionExecutionStartedEvent @event)
        {
            return _blockchainSettingsProvider.GetExclusiveWithdrawalsRequired(@event.BlockchainType);
        }
        
        private Task<TransactionExecutionAggregate> GetAggregateAsync(
            ITransactionExecutionEvent @event)
        {
            return _repository.GetAsync(@event.TransactionId);
        }
        
        protected async Task<TransactionExecutionAggregate> GetOrCreateAggregateAsync(
            TransactionExecutionStartedEvent @event)
        {
            var aggregate = await _repository.GetOrAddAsync(
                @event.TransactionId,
                () => TransactionExecutionAggregate.Start
                (
                    @event.OperationId,
                    @event.TransactionId,
                    @event.TransactionNumber,
                    @event.FromAddress,
                    @event.Outputs
                        .Select(e => e.ToDomain())
                        .ToArray(),
                    @event.BlockchainType,
                    @event.BlockchainAssetId,
                    @event.AssetId,
                    @event.IncludeFee
                ));
            
            _chaosKitty.Meow(@event.TransactionId);

            return aggregate;
        }
        
        protected Task MeowAndSaveAggregateAsync(
            TransactionExecutionAggregate aggregate)
        {
            _chaosKitty.Meow(aggregate.TransactionId);

            return _repository.SaveAsync(aggregate);
        }

        #region Handlers

        [UsedImplicitly]
        protected Task Handle(TransactionBuiltEvent evt, ICommandSender sender)
            => Handle(evt, sender, OnTransactionBuiltEventAsync);
        
        [UsedImplicitly]
        protected Task Handle(TransactionSignedEvent evt, ICommandSender sender)
            => Handle(evt, sender, OnTransactionSignedEventAsync);
        
        [UsedImplicitly]
        protected Task Handle(TransactionBroadcastedEvent evt, ICommandSender sender)
            => Handle(evt, sender, OnTransactionBroadcastedEventAsync);
        
        [UsedImplicitly]
        protected Task Handle(TransactionExecutionCompletedEvent evt, ICommandSender sender)
            => Handle(evt, sender, OnTransactionExecutionCompletedEventAsync);
        
        [UsedImplicitly]
        protected Task Handle(BroadcastedTransactionClearedEvent evt, ICommandSender sender)
            => Handle(evt, sender, OnBroadcastedTransactionClearedEventAsync);

        [UsedImplicitly]
        protected async Task Handle(TransactionBuildingRejectedEvent evt, ICommandSender sender)
        {
            // This event could be triggered only if process was splitted on several threads and one thread is stuck in Build step while
            // another go further and passed Broadcast step. Since stuck thread has blocked source address due to Build step retry,
            // we need to unconditionally release source address.
            
            var aggregate = await GetAggregateAsync(evt);

            await OnTransactionBuildingRejectedEventAsync(aggregate, sender);
        }
        
        [UsedImplicitly]
        protected Task Handle(TransactionExecutionFailedEvent evt, ICommandSender sender)
            => Handle(evt, sender, OnTransactionExecutionFailedEventAsync);
        
        [UsedImplicitly]
        protected Task Handle(TransactionExecutionRepeatRequestedEvent evt, ICommandSender sender)
            => Handle(evt, sender, OnTransactionExecutionRepeatRequestedEventAsync);

        protected async Task Handle<T>(
            T evt,
            ICommandSender sender,
            Func<TransactionExecutionAggregate, ICommandSender, Task> handler)
        
            where T : ITransactionExecutionEvent
        {
            var aggregate = await GetAggregateAsync(evt);
            
            if (_stateSwitcher.Switch(aggregate, evt))
            {
                await handler.Invoke(aggregate, sender);
            }
        }
        
        #endregion

        #region Event Processors
        
        protected abstract Task OnTransactionExecutionStartedEventAsync(
            TransactionExecutionAggregate aggregate,
            ICommandSender sender);

        protected virtual Task OnTransactionBuiltEventAsync(
            TransactionExecutionAggregate aggregate,
            ICommandSender sender)
        {
            sender.SendCommand
            (
                new SignTransactionCommand
                {
                    OperationId = aggregate.OperationId,
                    TransactionId = aggregate.TransactionId,
                    BlockchainType = aggregate.BlockchainType,
                    SignerAddress = aggregate.FromAddress,
                    TransactionContext = aggregate.Context
                },
                Self
            );
            
            return MeowAndSaveAggregateAsync(aggregate);
        }
        
        protected virtual Task OnTransactionSignedEventAsync(
            TransactionExecutionAggregate aggregate,
            ICommandSender sender)
        {
            sender.SendCommand
            (
                new BroadcastTransactionCommand
                {
                    OperationId = aggregate.OperationId,
                    TransactionId = aggregate.TransactionId,
                    TransactionNumber = aggregate.TransactionNumber,
                    BlockchainType = aggregate.BlockchainType,
                    SignedTransaction = aggregate.SignedTransaction
                },
                Self
            );

            return MeowAndSaveAggregateAsync(aggregate);
        }
        
        protected abstract Task OnTransactionBroadcastedEventAsync(
            TransactionExecutionAggregate aggregate,
            ICommandSender sender);
        
        protected abstract Task OnTransactionExecutionCompletedEventAsync(
            TransactionExecutionAggregate aggregate,
            ICommandSender sender);

        protected virtual Task OnBroadcastedTransactionClearedEventAsync(
            TransactionExecutionAggregate aggregate,
            ICommandSender sender)
        {
            return _repository.SaveAsync(aggregate);
        }
        
        protected abstract Task OnTransactionBuildingRejectedEventAsync(
            TransactionExecutionAggregate aggregate,
            ICommandSender sender);
        
        protected abstract Task OnTransactionExecutionFailedEventAsync(
            TransactionExecutionAggregate aggregate,
            ICommandSender sender);
        
        protected abstract Task OnTransactionExecutionRepeatRequestedEventAsync(
            TransactionExecutionAggregate aggregate,
            ICommandSender sender);
        
        #endregion
    }
}
