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
    [UsedImplicitly]
    public class TransactionWithNonExclusiveLocksExecutionSaga : TransactionExecutionSagaBase
    {
        public TransactionWithNonExclusiveLocksExecutionSaga(
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
            if (!ExclusiveWithdrawalsLockingRequired(evt))
            {
                var aggregate = await GetOrCreateAggregateAsync(evt);

                await OnTransactionExecutionStartedEventAsync(aggregate, sender);
            }
        }
        
        [UsedImplicitly]
        private Task Handle(SourceAddressLockedEvent evt, ICommandSender sender)
            => Handle(evt, sender, OnSourceAddressLockedEventAsync);
        
        [UsedImplicitly]
        private Task Handle(SourceAddressLockReleasedEvent evt, ICommandSender sender)
            => Handle(evt, sender, OnSourceAddressLockReleasedEventAsync);
        
        #endregion
        
        #region Event processors
        
        protected override Task OnTransactionExecutionStartedEventAsync(
            TransactionExecutionAggregate aggregate,
            ICommandSender sender)
        {
            sender.SendCommand
            (
                new LockSourceAddressCommand
                {
                    OperationId = aggregate.OperationId,
                    TransactionId = aggregate.TransactionId,
                    BlockchainType = aggregate.BlockchainType,
                    FromAddress = aggregate.FromAddress
                },
                Self
            );

            return Task.CompletedTask;
        }

        private Task OnSourceAddressLockedEventAsync(
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
                new ReleaseSourceAddressLockCommand
                {
                    OperationId = aggregate.OperationId,
                    TransactionId = aggregate.TransactionId,
                    BlockchainType = aggregate.BlockchainType,
                    FromAddress = aggregate.FromAddress
                },
                Self
            );
            
            return MeowAndSaveAggregateAsync(aggregate);
        }
        
        private Task OnSourceAddressLockReleasedEventAsync(
            TransactionExecutionAggregate aggregate,
            ICommandSender sender)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (aggregate.State)
            {
                case TransactionExecutionState.WaitingForEnding:
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
                    break;

                case TransactionExecutionState.SourceAddressReleased:
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
                    break;

                default:
                    throw new InvalidOperationException($"Unexpected aggregate state [{aggregate.State}]");
            }
                
            return MeowAndSaveAggregateAsync(aggregate);
        }

        protected override Task OnTransactionExecutionCompletedEventAsync(
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
                new ReleaseSourceAddressLockCommand
                {
                    OperationId = aggregate.OperationId,
                    TransactionId = aggregate.TransactionId,
                    BlockchainType = aggregate.BlockchainType,
                    FromAddress = aggregate.FromAddress,
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
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (aggregate.State)
            {
                case TransactionExecutionState.BuildingFailed:
                case TransactionExecutionState.BroadcastingFailed:
                    sender.SendCommand
                    (
                        new ReleaseSourceAddressLockCommand
                        {
                            OperationId = aggregate.OperationId,
                            TransactionId = aggregate.TransactionId,
                            BlockchainType = aggregate.BlockchainType,
                            FromAddress = aggregate.FromAddress
                        },
                        Self
                    );
                    break;

                case TransactionExecutionState.WaitingForEndingFailed:
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
                    break;

                default:
                    throw new InvalidOperationException($"Unexpected aggregate state [{aggregate.State}]");
            }
            
            return MeowAndSaveAggregateAsync(aggregate);
        }

        protected override Task OnTransactionExecutionRepeatRequestedEventAsync(
            TransactionExecutionAggregate aggregate,
            ICommandSender sender)
        {
            // ReSharper disable once SwitchStatementMissingSomeCases
            switch (aggregate.State)
            {
                case TransactionExecutionState.BroadcastingFailed:
                    sender.SendCommand
                    (
                        new ReleaseSourceAddressLockCommand
                        {
                            OperationId = aggregate.OperationId,
                            TransactionId = aggregate.TransactionId,
                            BlockchainType = aggregate.BlockchainType,
                            FromAddress = aggregate.FromAddress
                        },
                        Self
                    );
                    break;

                case TransactionExecutionState.WaitingForEndingFailed:
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
                    break;

                default:
                    throw new InvalidOperationException($"Unexpected aggregate state [{aggregate.State}]");
            }
            
            return MeowAndSaveAggregateAsync(aggregate);
        }
        
        #endregion
    }
}
