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
    public class TransactionExecutionSagaWithExclusiveLocks
    {
        private static string Self => CqrsModule.TransactionExecutorWithExclusiveLocks;
        
        private readonly IBlockchainSettingsProvider _blockchainSettingsProvider;
        private readonly IChaosKitty _chaosKitty;
        private readonly ITransactionExecutionsRepository _repository;
        private readonly IStateSwitcher<TransactionExecutionAggregate> _stateSwitcher;
        
        public TransactionExecutionSagaWithExclusiveLocks(
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

        #region Hanlders
        
        [UsedImplicitly]
        private async Task Handle(TransactionExecutionStartedEvent evt, ICommandSender sender)
        {
            if (!ExclusiveWithdrawalsLockingRequired(evt))
            {
                return;
            }
            
            var aggregate = await GetOrCreateAggregateAsync(evt);

            if (aggregate.State == TransactionExecutionState.Started)
            {
                if (aggregate.Outputs.Count > 1)
                {
                    throw new NotSupportedException("Exclusive withdrawals are not supported for transactions with multiple outputs.");
                }
                    
                sender.SendCommand
                (
                    new LockSourceAndTargetAddressesCommand
                    {
                        OperationId = aggregate.OperationId,
                        TransactionId = aggregate.TransactionId,
                        BlockchainType = aggregate.BlockchainType,
                        FromAddress = aggregate.FromAddress,
                        ToAddress = aggregate.Outputs.Single().Address
                    },
                    Self
                );
            }
        }

        [UsedImplicitly]
        private Task Handle(SourceAndTargetAddressesLockedEvent evt, ICommandSender sender)
            => Handle(evt, sender, OnSourceAndTargetAddressesLockedEvent);

        [UsedImplicitly]
        private Task Handle(TransactionBuiltEvent evt, ICommandSender sender)
            => Handle(evt, sender, OnTransactionBuiltEvent);

        [UsedImplicitly]
        private Task Handle(TransactionSignedEvent evt, ICommandSender sender)
            => Handle(evt, sender, OnTransactionSignedEvent);

        [UsedImplicitly]
        private Task Handle(TransactionBroadcastedEvent evt, ICommandSender sender)
            => Handle(evt, sender, OnTransactionBroadcastedEvent);

        [UsedImplicitly]
        private Task Handle(TransactionExecutionCompletedEvent evt, ICommandSender sender)
            => Handle(evt, sender, OnTransactionExecutionCompletedEvent);

        [UsedImplicitly]
        private Task Handle(SourceAndTargetAddressLocksReleasedEvent evt, ICommandSender sender)
            => Handle(evt, sender, OnSourceAndTargetAddressLocksReleasedEvent);

        [UsedImplicitly]
        private Task Handle(BroadcastedTransactionClearedEvent evt, ICommandSender sender)
            => Handle(evt, sender, OnBroadcastedTransactionClearedEvent);

        [UsedImplicitly]
        private Task Handle(TransactionExecutionFailedEvent evt, ICommandSender sender)
            => Handle(evt, sender, OnTransactionExecutionFailedEvent);

        [UsedImplicitly]
        private Task Handle(TransactionExecutionRepeatRequestedEvent evt, ICommandSender sender)
            => Handle(evt, sender, OnTransactionExecutionRepeatRequestedEvent);

        [UsedImplicitly]
        private async Task Handle(TransactionBuildingRejectedEvent evt, ICommandSender sender)
        {
            var aggregate = await GetAggregateAsync(evt);
            
            // This event could be triggered only if process was split on several threads and one thread is stuck in Build step while
            // another go further and passed Broadcast step. Since stuck thread has blocked source address due to Build step retry,
            // we need to unconditionally release source address.
            
            sender.SendCommand
            (
                new ReleaseSourceAndTargetAddressLocksCommand
                {
                    OperationId = aggregate.OperationId,
                    TransactionId = aggregate.TransactionId,
                    BlockchainType = aggregate.BlockchainType,
                    FromAddress = aggregate.FromAddress,
                    ToAddress = aggregate.Outputs.Single().Address,
                    AbortWorkflow = true
                }, 
                Self
            );
        }

        #endregion
        
        #region Event processors

        private static void OnSourceAndTargetAddressesLockedEvent(
            TransactionExecutionAggregate aggregate,
            ICommandSender sender)
        {
            sender.SendCommand
            (
                new BuildTransactionCommand
                {
                    OperationId = aggregate.OperationId,
                    TransactionId = aggregate.TransactionId,
                    TransactionNumber = aggregate.TransactionNumber,
                    BlockchainType = aggregate.BlockchainType,
                    BlockchainAssetId = aggregate.BlockchainAssetId,
                    FromAddress = aggregate.FromAddress,
                    Outputs = aggregate.Outputs
                        .Select(e => e.ToContract())
                        .ToArray(),
                    IncludeFee = aggregate.IncludeFee
                },
                Self
            );
        }

        private static void OnTransactionBuiltEvent(
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
        }

        private static void OnTransactionBroadcastedEvent(
            TransactionExecutionAggregate aggregate,
            ICommandSender sender)
        {
            sender.SendCommand
            (
                new WaitForTransactionEndingCommand
                {
                    OperationId = aggregate.OperationId,
                    TransactionId = aggregate.TransactionId,
                    TransactionNumber = aggregate.TransactionNumber,
                    BlockchainType = aggregate.BlockchainType,
                    BlockchainAssetId = aggregate.BlockchainAssetId,
                    Outputs = aggregate.Outputs
                        .Select(o => o.ToContract())
                        .ToArray()
                },
                Self
            );
        }

        private static void OnTransactionExecutionCompletedEvent(
            TransactionExecutionAggregate aggregate,
            ICommandSender sender)
        {
            sender.SendCommand
            (
                new ReleaseSourceAndTargetAddressLocksCommand
                {
                    OperationId = aggregate.OperationId,
                    TransactionId = aggregate.TransactionId,
                    BlockchainType = aggregate.BlockchainType,
                    FromAddress = aggregate.FromAddress,
                    ToAddress = aggregate.Outputs.Single().Address
                }, 
                Self
            );
        }

        private static void OnSourceAndTargetAddressLocksReleasedEvent(
            TransactionExecutionAggregate aggregate,
            ICommandSender sender)
        {
            sender.SendCommand
            (
                new ClearBroadcastedTransactionCommand
                {
                    OperationId = aggregate.OperationId,
                    TransactionId = aggregate.TransactionId,
                    BlockchainType = aggregate.BlockchainType
                },
                Self
            );
        }

        private static void OnTransactionExecutionFailedEvent(
            TransactionExecutionAggregate aggregate,
            ICommandSender sender)
        {
            sender.SendCommand
            (
                new ReleaseSourceAndTargetAddressLocksCommand
                {
                    OperationId = aggregate.OperationId,
                    TransactionId = aggregate.TransactionId,
                    BlockchainType = aggregate.BlockchainType,
                    FromAddress = aggregate.FromAddress,
                    ToAddress = aggregate.Outputs.Single().Address
                }, 
                Self
            );
        }

        private static void OnTransactionExecutionRepeatRequestedEvent(
            TransactionExecutionAggregate aggregate,
            ICommandSender sender)
        {
            sender.SendCommand
            (
                new ReleaseSourceAndTargetAddressLocksCommand
                {
                    OperationId = aggregate.OperationId,
                    TransactionId = aggregate.TransactionId,
                    BlockchainType = aggregate.BlockchainType,
                    FromAddress = aggregate.FromAddress,
                    ToAddress = aggregate.Outputs.Single().Address
                }, 
                Self
            );
        }

        private static void OnTransactionSignedEvent(
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
        }
        
        private static void OnBroadcastedTransactionClearedEvent(
            TransactionExecutionAggregate aggregate,
            ICommandSender sender)
        {
            
        }
        
        #endregion
        
        #region Helpers
        
        private bool ExclusiveWithdrawalsLockingRequired(
            TransactionExecutionStartedEvent @event)
        {
            return _blockchainSettingsProvider.GetExclusiveWithdrawalsRequired(@event.BlockchainType);
        }
        
        private Task<TransactionExecutionAggregate> GetAggregateAsync(
            ITransactionExecutionEvent @event)
        {
            return _repository.GetAsync(@event.TransactionId);
        }
        
        private async Task<TransactionExecutionAggregate> GetOrCreateAggregateAsync(
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
        
        private async Task Handle<T>(
            T evt,
            ICommandSender sender,
            Action<TransactionExecutionAggregate, ICommandSender> handler)
        
            where T : ITransactionExecutionEvent
        {
            var aggregate = await GetAggregateAsync(evt);
            
            if (_stateSwitcher.Switch(aggregate, evt))
            {
                handler.Invoke(aggregate, sender);

                _chaosKitty.Meow(aggregate.TransactionId);

                await _repository.SaveAsync(aggregate);
            }
        }
        
        #endregion
    }
}
