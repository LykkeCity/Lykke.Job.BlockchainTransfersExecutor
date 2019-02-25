using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains;
using Lykke.Job.BlockchainOperationsExecutor.Mappers;
using Lykke.Job.BlockchainOperationsExecutor.StateMachine;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.TransactionExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Sagas
{
    public class TransactionWithExclusiveLocksExecutionSaga : TransactionExecutionSagaBase
    {
        public TransactionWithExclusiveLocksExecutionSaga(
            IBlockchainSettingsProvider blockchainSettingsProvider,
            IChaosKitty chaosKitty,
            ITransactionExecutionsRepository repository,
            IStateSwitcher<TransactionExecutionAggregate> stateSwitcher) 
            
            : base(blockchainSettingsProvider, chaosKitty, repository, stateSwitcher)
        {
            
        }

        #region Hanlders
        
        [UsedImplicitly]
        private async Task Handle(TransactionExecutionStartedEvent evt, ICommandSender sender)
        {
            if (ExclusiveWithdrawalsLockingRequired(evt))
            {
                var aggregate = await GetOrCreateAggregateAsync(evt);

                await OnTransactionExecutionStartedEventAsync(aggregate, sender);
            }
        }
        
        [UsedImplicitly]
        private Task Handle(SourceAndTargetAddressesLockedEvent evt, ICommandSender sender)
            => Handle(evt, sender, OnSourceAndTargetAddressesLockedEventAsync);
        
        [UsedImplicitly]
        private Task Handle(SourceAndTargetAddressLocksReleasedEvent evt, ICommandSender sender)
            => Handle(evt, sender, OnSourceAndTargetAddressLocksReleasedEventAsync);
        
        #endregion
        
        #region Event processors
        
        protected override Task OnTransactionExecutionStartedEventAsync(
            TransactionExecutionAggregate aggregate,
            ICommandSender sender)
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

            return Task.CompletedTask;
        }

        private Task OnSourceAndTargetAddressesLockedEventAsync(
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

            return MeowAndSaveAggregateAsync(aggregate);
        }
        
        protected override Task OnTransactionBroadcastedEventAsync(
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
            
            return MeowAndSaveAggregateAsync(aggregate);
        }

        protected override Task OnTransactionExecutionCompletedEventAsync(
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
            
            return MeowAndSaveAggregateAsync(aggregate);
        }

        private Task OnSourceAndTargetAddressLocksReleasedEventAsync(
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
                
            return MeowAndSaveAggregateAsync(aggregate);
        }

        protected override Task OnTransactionBuildingRejectedEventAsync(
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
                    ToAddress = aggregate.Outputs.Single().Address,
                    AbortWorkflow = true
                }, 
                Self
            );

            return Task.CompletedTask;
        }

        protected override Task OnTransactionExecutionFailedEventAsync(
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
            
            return MeowAndSaveAggregateAsync(aggregate);
        }

        protected override Task OnTransactionExecutionRepeatRequestedEventAsync(
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
            
            return MeowAndSaveAggregateAsync(aggregate);
        }
        
        #endregion
    }
}
