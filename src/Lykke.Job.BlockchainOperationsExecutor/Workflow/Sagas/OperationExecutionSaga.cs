using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Sagas
{
    /// <summary>
    /// -> StartOperationCommand
    /// -> OperationStartRequestedEvent
    ///     -> BuildTransactionCommand
    /// -> TransactionBuiltEvent
    ///     -> SignTransactionCommand
    /// -> TransactionSignedEvent
    ///     -> BroadcastTransactionCommand
    /// -> TransactionBroadcasted
    ///     -> WaitForTransactionEndingCommand
    /// -> OperationCompletedEvent | OperationFailedEvent
    ///     -> ReleaseSourceAddressLockCommand
    /// -> SourceAddressLockReleasedEvent
    /// </summary>
    [UsedImplicitly]
    public class OperationExecutionSaga
    {
        private static string Self => BlockchainOperationsExecutorBoundedContext.Name;

        private readonly IOperationExecutionsRepository _repository;

        public OperationExecutionSaga(IOperationExecutionsRepository repository)
        {
            _repository = repository;
        }

        [UsedImplicitly]
        private async Task Handle(OperationStartRequestedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetOrAddAsync(
                evt.OperationId,
                () => OperationExecutionAggregate.CreateNew(
                    evt.OperationId,
                    evt.BlockchainType,
                    evt.FromAddress,
                    evt.ToAddress,
                    evt.AssetId,
                    evt.Amount,
                    evt.IncludeFee));

            ChaosKitty.Meow();

            if (aggregate.State == OperationExecutionState.Started)
            {
                sender.SendCommand(new BuildTransactionCommand
                    {
                        OperationId = aggregate.OperationId,
                        BlockchainType = aggregate.BlockchainType,
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
            
            if (aggregate.OnTransactionBuilt(evt.TransactionContext, evt.BlockchainAssetId))
            {
                sender.SendCommand(new SignTransactionCommand
                    {
                        BlockchainType = aggregate.BlockchainType,
                        OperationId = aggregate.OperationId,
                        SignerAddress = aggregate.FromAddress,
                        TransactionContext = aggregate.TransactionContext
                    },
                    Self);

                ChaosKitty.Meow();

                await _repository.SaveAsync(aggregate);

                ChaosKitty.Meow();
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

                ChaosKitty.Meow();

                await _repository.SaveAsync(aggregate);

                ChaosKitty.Meow();
            }
        }

        [UsedImplicitly]
        private async Task Handle(TransactionBroadcastedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (aggregate.OnTransactionBroadcasted())
            {
                if (!aggregate.TransactionBroadcastingMoment.HasValue)
                {
                    throw new InvalidOperationException("TransactionBroadcastingMoment should be not null at this moment");
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

                ChaosKitty.Meow();

                await _repository.SaveAsync(aggregate);

                ChaosKitty.Meow();
            }
        }

        [UsedImplicitly]
        private async Task Handle(OperationCompletedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (aggregate.OnTransactionCompleted(evt.TransactionHash, evt.TransactionTimestamp, evt.Fee))
            {
                sender.SendCommand(new ReleaseSourceAddressLockCommand
                    {
                        BlockchainType = aggregate.BlockchainType,
                        FromAddress = aggregate.FromAddress,
                        OperationId = aggregate.OperationId
                    },
                    Self);

                ChaosKitty.Meow();

                await _repository.SaveAsync(aggregate);

                ChaosKitty.Meow();
            }
        }

        [UsedImplicitly]
        private async Task Handle(OperationFailedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (aggregate.OnTransactionFailed(evt.TransactionTimestamp, evt.Error))
            {
                sender.SendCommand(new ReleaseSourceAddressLockCommand
                    {
                        BlockchainType = aggregate.BlockchainType,
                        FromAddress = aggregate.FromAddress,
                        OperationId = aggregate.OperationId
                    },
                    Self);

                ChaosKitty.Meow();

                await _repository.SaveAsync(aggregate);

                ChaosKitty.Meow();
            }
        }

        [UsedImplicitly]
        private async Task Handle(SourceAddressLockReleasedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (aggregate.OnSourceAddressLockReleased())
            {
                await _repository.SaveAsync(aggregate);

                ChaosKitty.Meow();
            }
        }
    }
}
