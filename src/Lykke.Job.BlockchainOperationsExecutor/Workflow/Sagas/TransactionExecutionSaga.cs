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
    [UsedImplicitly]
    public class TransactionExecutionSaga
    {
        private static string Self => CqrsModule.TransactionExecutor;

        private readonly IBlockchainSettingsProvider _blockchainSettingsProvider;
        private readonly IChaosKitty _chaosKitty;
        private readonly ITransactionExecutionsRepository _repository;
        private readonly IStateSwitcher<TransactionExecutionAggregate> _stateSwitcher;

        public TransactionExecutionSaga(
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

        [UsedImplicitly]
        private async Task Handle(TransactionExecutionStartedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetOrAddAsync(
                evt.TransactionId,
                () => TransactionExecutionAggregate.Start(
                    evt.OperationId,
                    evt.TransactionId,
                    evt.TransactionNumber,
                    evt.FromAddress,
                    evt.Outputs
                        .Select(e => e.ToDomain())
                        .ToArray(),
                    evt.BlockchainType,
                    evt.BlockchainAssetId,
                    evt.AssetId,
                    evt.IncludeFee));

            _chaosKitty.Meow(evt.TransactionId);

            if (aggregate.State == TransactionExecutionState.Started)
            {
                if (_blockchainSettingsProvider.GetIsExclusiveWithdrawalsRequired(aggregate.BlockchainType))
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
                else
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
                }
            }
        }

        [UsedImplicitly]
        private async Task Handle(SourceAddressLockedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.TransactionId);
            
            if (_stateSwitcher.Switch(aggregate, evt))
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

                _chaosKitty.Meow(evt.TransactionId);

                await _repository.SaveAsync(aggregate);
            }
        }
        
        [UsedImplicitly]
        private async Task Handle(SourceAndTargetAddressesLockedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.TransactionId);
            
            if (_stateSwitcher.Switch(aggregate, evt))
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

                _chaosKitty.Meow(evt.TransactionId);

                await _repository.SaveAsync(aggregate);
            }
        }
        
        [UsedImplicitly]
        private async Task Handle(TransactionBuiltEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.TransactionId);
            
            if (_stateSwitcher.Switch(aggregate, evt))
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

                _chaosKitty.Meow(evt.TransactionId);

                await _repository.SaveAsync(aggregate);
            }
        }

        [UsedImplicitly]
        private async Task Handle(TransactionSignedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.TransactionId);

            if (_stateSwitcher.Switch(aggregate, evt))
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

                _chaosKitty.Meow(evt.TransactionId);

                await _repository.SaveAsync(aggregate);
            }
        }

        [UsedImplicitly]
        private async Task Handle(TransactionBroadcastedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.TransactionId);

            if (_stateSwitcher.Switch(aggregate, evt))
            {
                if (_blockchainSettingsProvider.GetIsExclusiveWithdrawalsRequired(aggregate.BlockchainType))
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
                else
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
                }
                
                _chaosKitty.Meow(evt.TransactionId);

                await _repository.SaveAsync(aggregate);
            }
        }

        [UsedImplicitly]
        private async Task Handle(SourceAddressLockReleasedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.TransactionId);

            if (_stateSwitcher.Switch(aggregate, evt))
            {
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
                
                _chaosKitty.Meow(evt.TransactionId);

                await _repository.SaveAsync(aggregate);
            }
        }

        [UsedImplicitly]
        private async Task Handle(TransactionExecutionCompletedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.TransactionId);

            if (_stateSwitcher.Switch(aggregate, evt))
            {
                if (_blockchainSettingsProvider.GetIsExclusiveWithdrawalsRequired(aggregate.BlockchainType))
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
                else
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
                
                _chaosKitty.Meow(evt.TransactionId);

                await _repository.SaveAsync(aggregate);
            }
        }

        [UsedImplicitly]
        private async Task Handle(SourceAndTargetAddressLocksReleasedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.TransactionId);

            if (_stateSwitcher.Switch(aggregate, evt))
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
                
                _chaosKitty.Meow(evt.TransactionId);

                await _repository.SaveAsync(aggregate);
            }
        }

        [UsedImplicitly]
        private async Task Handle(BroadcastedTransactionClearedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.TransactionId);

            if (_stateSwitcher.Switch(aggregate, evt))
            {
                await _repository.SaveAsync(aggregate);
            }
        }

        [UsedImplicitly]
        private async Task Handle(TransactionBuildingRejectedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.TransactionId);

            // This event could be triggered only if process was splitted on several threads and one thread is stuck in Build step while
            // another go futher and passed Broadcast step. Since stuck thread has blocked source addres due to Build step retry,
            // we need to unconditionally release source address.

            if (_blockchainSettingsProvider.GetIsExclusiveWithdrawalsRequired(aggregate.BlockchainType))
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
            }
            else
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
            }
        }

        [UsedImplicitly]
        private async Task Handle(TransactionExecutionFailedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.TransactionId);

            if (_stateSwitcher.Switch(aggregate, evt))
            {
                if (_blockchainSettingsProvider.GetIsExclusiveWithdrawalsRequired(aggregate.BlockchainType))
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
                else
                {
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
                }
                
                _chaosKitty.Meow(evt.TransactionId);

                await _repository.SaveAsync(aggregate);
            }
        }

        [UsedImplicitly]
        private async Task Handle(TransactionExecutionRepeatRequestedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.TransactionId);

            if (_stateSwitcher.Switch(aggregate, evt))
            {
                if (_blockchainSettingsProvider.GetIsExclusiveWithdrawalsRequired(aggregate.BlockchainType))
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
                else
                {
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
                }
                
                _chaosKitty.Meow(evt.TransactionId);

                await _repository.SaveAsync(aggregate);
            }
        }
    }
}
