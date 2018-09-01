using System;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.OperationExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Mappers;
using Lykke.Job.BlockchainOperationsExecutor.Modules;
using Lykke.Job.BlockchainOperationsExecutor.StateMachine;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.OperationExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.TransactionExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.OperationExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Sagas
{
    [UsedImplicitly]
    public class OperationExecutionSaga
    {
        private static string Self => BlockchainOperationsExecutorBoundedContext.Name;

        private readonly IChaosKitty _chaosKitty;
        private readonly IOperationExecutionsRepository _repository;
        private readonly IStateSwitcher<OperationExecutionAggregate> _stateSwitcher;

        public OperationExecutionSaga(
            IChaosKitty chaosKitty,
            IOperationExecutionsRepository repository,
            IActiveTransactionsRepository activeTransactionsRepository,
            IStateSwitcher<OperationExecutionAggregate> stateSwitcher)
        {
            _chaosKitty = chaosKitty;
            _repository = repository;
            _stateSwitcher = stateSwitcher;
        }

        [UsedImplicitly]
        private async Task Handle(OperationExecutionStartedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetOrAddAsync
            (
                evt.OperationId,
                () => OperationExecutionAggregate.Start
                (
                    evt.OperationId,
                    evt.FromAddress,
                    evt.Outputs
                        .Select(e => e.FromContract())
                        .ToArray(),
                    evt.AssetId,
                    evt.IncludeFee,
                    evt.BlockchainType,
                    evt.BlockchainAssetId
                )
            );

            _chaosKitty.Meow(evt.OperationId);

            if (aggregate.State == OperationExecutionState.Started)
            {
                _chaosKitty.Meow(evt.OperationId);

                sender.SendCommand
                (
                    new GenerateActiveTransactionIdCommand
                    {
                        OperationId = aggregate.OperationId,
                        ActiveTransactioNumber = aggregate.ActiveTransactionNumber
                    },
                    Self
                );
            }
        }

        [UsedImplicitly]
        private async Task Handle(ActiveTransactionIdGeneratedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (_stateSwitcher.Switch(aggregate, evt))
            {
                if (!aggregate.ActiveTransactionId.HasValue)
                {
                    throw new InvalidOperationException("Active transaction id should be not null here");
                }

                sender.SendCommand
                (
                    new StartTransactionExecutionCommand
                    {
                        OperationId = aggregate.OperationId,
                        TransactionId = aggregate.ActiveTransactionId.Value,
                        TransactionNumber = aggregate.ActiveTransactionNumber,
                        BlockchainType = aggregate.BlockchainType,
                        BlockchainAssetId = aggregate.BlockchainAssetId,
                        FromAddress = aggregate.FromAddress,
                        Outputs = aggregate.Outputs
                            .Select(e => e.ToContract())
                            .ToArray(),
                        AssetId = aggregate.AssetId,
                        IncludeFee = aggregate.IncludeFee
                    },
                    CqrsModule.TransactionExecutor
                );

                _chaosKitty.Meow(aggregate.OperationId);

                await _repository.SaveAsync(aggregate);
            }
        }

        [UsedImplicitly]
        private async Task Handle(TransactionExecutionStartedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (_stateSwitcher.Switch(aggregate, evt))
            {
                await _repository.SaveAsync(aggregate);
            }
        }

        [UsedImplicitly]
        private async Task Handle(TransactionExecutionCompletedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (_stateSwitcher.Switch(aggregate, evt))
            {
                if (!aggregate.ActiveTransactionId.HasValue)
                {
                    throw new InvalidOperationException("Active transaction id should be not null here");
                }

                sender.SendCommand
                (
                    new NotifyOperationExecutionCompletedCommand
                    {
                        OperationId = aggregate.OperationId,
                        TransactionId = aggregate.ActiveTransactionId.Value,
                        TransactionAmount = aggregate.TransactionAmount,
                        TransactionBlock = aggregate.TransactionBlock,
                        TransactionFee = aggregate.TransactionFee,
                        TransactionHash = aggregate.TransactionHash
                    },
                    Self
                );

                _chaosKitty.Meow(aggregate.OperationId);

                await _repository.SaveAsync(aggregate);
            }
        }

        [UsedImplicitly]
        private async Task Handle(TransactionExecutionFailedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (_stateSwitcher.Switch(aggregate, evt))
            {
                if (!aggregate.Result.HasValue)
                {
                    throw new InvalidOperationException("Result should be not null here");
                }

                if (aggregate.Result.Value == OperationExecutionResult.Completed)
                {
                    throw new InvalidOperationException($"Result can't be {nameof(OperationExecutionResult.Completed)} here" );
                }

                if (!aggregate.ActiveTransactionId.HasValue)
                {
                    throw new InvalidOperationException("Active transaction id should be not null here");
                }

                sender.SendCommand
                (
                    new NotifyOperationExecutionFailedCommand
                    {
                        OperationId = aggregate.OperationId,
                        TransactionId = aggregate.ActiveTransactionId.Value,
                        Error = aggregate.Error,
                        ErrorCode = aggregate.Result.Value.MapToOperationExecutionErrorCode()
                    },
                    Self
                );

                _chaosKitty.Meow(aggregate.OperationId);

                await _repository.SaveAsync(aggregate);
            }
        }

        [UsedImplicitly]
        private async Task Handle(TransactionExecutionRepeatRequestedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (_stateSwitcher.Switch(aggregate, evt))
            {
                if (!aggregate.ActiveTransactionId.HasValue)
                {
                    throw new InvalidOperationException("Active transaction execution should be not null here");
                }

                sender.SendCommand
                (
                    new ClearActiveTransactionCommand
                    {
                        OperationId = aggregate.OperationId,
                        TransactionId = aggregate.ActiveTransactionId.Value,
                        TransactionNumber = aggregate.ActiveTransactionNumber
                    },
                    Self
                );

                _chaosKitty.Meow(aggregate.OperationId);

                await _repository.SaveAsync(aggregate);
            }
        }

        [UsedImplicitly]
        private async Task Handle(ActiveTransactionClearedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (_stateSwitcher.Switch(aggregate, evt))
            {
                sender.SendCommand
                (
                    new GenerateActiveTransactionIdCommand
                    {
                        OperationId = aggregate.OperationId,
                        ActiveTransactioNumber = aggregate.ActiveTransactionNumber
                    },
                    Self
                );

                _chaosKitty.Meow(aggregate.OperationId);

                await _repository.SaveAsync(aggregate);
            }
        }

        [UsedImplicitly]
        private async Task Handle(OperationExecutionCompletedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (_stateSwitcher.Switch(aggregate, evt))
            {
                await _repository.SaveAsync(aggregate);
            }
        }

        [UsedImplicitly]
        private async Task Handle(OperationExecutionFailedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (_stateSwitcher.Switch(aggregate, evt))
            {
                await _repository.SaveAsync(aggregate);
            }
        }
    }
}
