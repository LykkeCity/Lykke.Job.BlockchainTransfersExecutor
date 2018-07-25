using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Sagas
{
    /// <summary>
    /// -> StartOperationExecutionCommand
    /// -> OperationExecutionStartedEvent
    ///     -> BuildTransactionCommand
    /// -> TransactionBuiltEvent                    | TransactionBuildingRejectedEvent
    ///     -> SignTransactionCommand               | -> ReleaseSourceAddressLockCommand
    /// -> TransactionSignedEvent
    ///     -> BroadcastTransactionCommand
    /// -> TransactionBroadcastedEvent
    ///     -> ReleaseSourceAddressLockCommand
    /// -> SourceAddressLockReleasedEvent
    ///     -> WaitForTransactionEndingCommand
    /// -> OperationExecutionCompletedEvent         | OperationExecutionFailedEvent
    ///     -> ForgetBroadcastedTransactionCommand
    /// -> BroadcastedTransactionForgottenEvent
    /// </summary>
    [UsedImplicitly]
    public class OperationExecutionSaga
    {
        private static string Self => BlockchainOperationsExecutorBoundedContext.Name;

        private readonly IChaosKitty _chaosKitty;
        private readonly IOperationExecutionsRepository _repository;

        public OperationExecutionSaga(
            IChaosKitty chaosKitty,
            IOperationExecutionsRepository repository)
        {
            _chaosKitty = chaosKitty;
            _repository = repository;
        }

        [UsedImplicitly]
        private async Task Handle(OperationExecutionStartedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetOrAddAsync(
                evt.OperationId,
                () => OperationExecutionAggregate.CreateNew(
                    evt.OperationId,
                    evt.FromAddress,
                    evt.ToAddress,
                    evt.BlockchainType,
                    evt.BlockchainAssetId,
                    evt.AssetId,
                    evt.Amount,
                    evt.IncludeFee));

            _chaosKitty.Meow(evt.OperationId);

            if (aggregate.State == OperationExecutionState.Started)
            {
                sender.SendCommand(new BuildTransactionCommand
                    {
                        BlockchainType = aggregate.BlockchainType,
                        BlockchainAssetId = aggregate.BlockchainAssetId,
                        OperationId = aggregate.OperationId,
                        FromAddress = aggregate.FromAddress,
                        ToAddress = aggregate.ToAddress,
                        AssetId = aggregate.AssetId,
                        Amount = aggregate.Amount,
                        IncludeFee = aggregate.IncludeFee
                    },
                    Self);
            }
        }

        [UsedImplicitly]
        private async Task Handle(TransactionBuiltEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (aggregate.OnTransactionBuilt(evt.FromAddressContext, evt.TransactionContext, evt.BlockchainType, evt.BlockchainAssetId))
            {
                sender.SendCommand(new SignTransactionCommand
                    {
                        BlockchainType = aggregate.BlockchainType,
                        OperationId = aggregate.OperationId,
                        SignerAddress = aggregate.FromAddress,
                        TransactionContext = aggregate.TransactionContext
                    },
                    Self);

                _chaosKitty.Meow(evt.OperationId);

                await _repository.SaveAsync(aggregate);
            }
        }

        [UsedImplicitly]
        private async Task Handle(TransactionBuildingRejectedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (aggregate.OnTransactionBuildingRejected())
            {
                sender.SendCommand(new ReleaseSourceAddressLockCommand
                    {
                        BlockchainType = aggregate.BlockchainType,
                        OperationId = aggregate.OperationId,
                        FromAddress = aggregate.FromAddress
                    },
                    Self);

                _chaosKitty.Meow(evt.OperationId);

                await _repository.SaveAsync(aggregate);
            }
        }

        [UsedImplicitly]
        private async Task Handle(TransactionSignedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (aggregate.OnTransactionSigned(evt.SignedTransaction))
            {
                sender.SendCommand(new BroadcastTransactionCommand
                    {
                        BlockchainType = aggregate.BlockchainType,
                        OperationId = aggregate.OperationId,
                        SignedTransaction = aggregate.SignedTransaction
                    },
                    Self);

                _chaosKitty.Meow(evt.OperationId);

                await _repository.SaveAsync(aggregate);
            }
        }

        [UsedImplicitly]
        private async Task Handle(TransactionBroadcastedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (aggregate.OnTransactionBroadcasted())
            {
                sender.SendCommand(new ReleaseSourceAddressLockCommand
                    {
                        BlockchainType = aggregate.BlockchainType,
                        FromAddress = aggregate.FromAddress,
                        OperationId = aggregate.OperationId
                    },
                    Self);

                _chaosKitty.Meow(evt.OperationId);

                await _repository.SaveAsync(aggregate);
            }
        }

        [UsedImplicitly]
        private async Task Handle(SourceAddressLockReleasedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (aggregate.OnSourceAddressLockReleased())
            {
                if (!aggregate.TransactionBroadcastingMoment.HasValue)
                {
                    throw new InvalidOperationException(
                        "TransactionBroadcastingMoment should be not null at this moment");
                }

                sender.SendCommand(new WaitForTransactionEndingCommand
                    {
                        BlockchainType = aggregate.BlockchainType,
                        BlockchainAssetId = aggregate.BlockchainAssetId,
                        OperationId = aggregate.OperationId,
                        OperationStartMoment = aggregate.StartMoment,
                        TransactionBroadcastingMoment = aggregate.TransactionBroadcastingMoment.Value
                    },
                    Self);

                _chaosKitty.Meow(evt.OperationId);

                await _repository.SaveAsync(aggregate);
            }
        }

        [UsedImplicitly]
        private async Task Handle(OperationExecutionCompletedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (aggregate.OnTransactionCompleted(evt.TransactionHash, evt.Block, evt.Fee))
            {
                sender.SendCommand(new ForgetBroadcastedTransactionCommand
                    {
                        BlockchainType = aggregate.BlockchainType,
                        OperationId = aggregate.OperationId,
                        FromAddress = aggregate.FromAddress,
                        ToAddress = aggregate.ToAddress
                    },
                    Self);

                _chaosKitty.Meow(evt.OperationId);

                await _repository.SaveAsync(aggregate);
            }
        }

        [UsedImplicitly]
        private async Task Handle(OperationExecutionFailedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (aggregate.OnTransactionFailed(evt.Error))
            {
                sender.SendCommand(new ForgetBroadcastedTransactionCommand
                    {
                        BlockchainType = aggregate.BlockchainType,
                        OperationId = aggregate.OperationId,
                        FromAddress = aggregate.FromAddress,
                        ToAddress = aggregate.ToAddress
                    },
                    Self);

                _chaosKitty.Meow(evt.OperationId);

                await _repository.SaveAsync(aggregate);
            }
        }

        [UsedImplicitly]
        private async Task Handle(BroadcastedTransactionForgottenEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (aggregate.OnBroadcastedTransactionForgotten())
            {
                await _repository.SaveAsync(aggregate);
            }
        }
    }
}
