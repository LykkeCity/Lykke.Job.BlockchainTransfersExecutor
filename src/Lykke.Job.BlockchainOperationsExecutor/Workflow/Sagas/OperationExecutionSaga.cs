using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Sagas
{
    /// <summary>
    /// -> StartOperationExecutionCommand
    /// -> OperationExecutionStartedEvent
    ///     -> BuildTransactionCommand
    /// -> TransactionBuiltEvent
    ///     -> SignTransactionCommand
    /// -> TransactionSignedEvent
    ///     -> BroadcastTransactionCommand
    /// -> TransactionBroadcastedEvent
    ///     -> WaitForTransactionEndingCommand
    /// -> OperationExecutionCompletedEvent | OperationExecutionFailedEvent
    ///     -> ReleaseSourceAddressLockCommand
    /// -> SourceAddressLockReleasedEvent
    ///     -> ForgetBroadcastedTransactionCommand
    /// -> BroadcastedTransactionForgottenEvent
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
        private async Task Handle(OperationExecutionStartedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetOrAddAsync(
                evt.OperationId,
                () => OperationExecutionAggregate.CreateNew(
                    evt.OperationId,
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
            
            if (aggregate.OnTransactionBuilt(evt.TransactionContext, evt.BlockchainType, evt.BlockchainAssetId))
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
        private async Task Handle(OperationExecutionCompletedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (aggregate.OnTransactionCompleted(evt.TransactionHash, evt.Fee))
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
        private async Task Handle(OperationExecutionFailedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (aggregate.OnTransactionFailed(evt.Error))
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
                sender.SendCommand(new ForgetBroadcastedTransactionCommand
                    {
                        BlockchainType = aggregate.BlockchainType,
                        OperationId = aggregate.OperationId
                    },
                    Self);

                await _repository.SaveAsync(aggregate);

                ChaosKitty.Meow();
            }
        }

        [UsedImplicitly]
        private async Task Handle(BroadcastedTransactionForgottenEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (aggregate.OnBroadcastedTransactionForgotten())
            {
                await _repository.SaveAsync(aggregate);

                ChaosKitty.Meow();
            }
        }
    }
}
