using System;
using System.Threading.Tasks;
using Common.Log;
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
    /// -> TransactionBuiltEvent
    ///     -> SignTransactionCommand
    /// -> TransactionSignedEvent
    ///     -> BroadcastTransactionCommand
    /// -> TransactionBroadcastedEvent
    ///     -> ReleaseSourceAddressLockCommand
    /// -> SourceAddressLockReleasedEvent
    ///     -> WaitForTransactionEndingCommand
    /// -> OperationExecutionCompletedEvent | OperationExecutionFailedEvent
    ///     -> ForgetBroadcastedTransactionCommand
    /// -> BroadcastedTransactionForgottenEvent
    /// </summary>
    [UsedImplicitly]
    public class OperationExecutionSaga
    {
        private static string Self => BlockchainOperationsExecutorBoundedContext.Name;

        private readonly IChaosKitty _chaosKitty;
        private readonly ILog _log;
        private readonly IOperationExecutionsRepository _repository;

        public OperationExecutionSaga(
            IChaosKitty chaosKitty,
            ILog log, 
            IOperationExecutionsRepository repository)
        {
            _chaosKitty = chaosKitty;
            _log = log;
            _repository = repository;
        }

        [UsedImplicitly]
        private async Task Handle(OperationExecutionStartedEvent evt, ICommandSender sender)
        {

            _log.WriteInfo(nameof(OperationExecutionStartedEvent), evt, "");


            try
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

                _chaosKitty.Meow(evt.OperationId);

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
            catch (Exception ex)
            {
                _log.WriteError(nameof(OperationExecutionStartedEvent), evt, ex);
                throw;
            }
        }

        [UsedImplicitly]
        private async Task Handle(TransactionBuiltEvent evt, ICommandSender sender)
        {

            _log.WriteInfo(nameof(TransactionBuiltEvent), evt, "");

            try
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
            catch (Exception ex)
            {
                _log.WriteError(nameof(TransactionBuiltEvent), evt, ex);
                throw;
            }
        }

        [UsedImplicitly]
        private async Task Handle(TransactionSignedEvent evt, ICommandSender sender)
        {

            _log.WriteInfo(nameof(TransactionSignedEvent), evt, "");

            try
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
            catch (Exception ex)
            {
                _log.WriteError(nameof(TransactionSignedEvent), evt, ex);
                throw;
            }
        }

        [UsedImplicitly]
        private async Task Handle(TransactionBroadcastedEvent evt, ICommandSender sender)
        {

            _log.WriteInfo(nameof(TransactionBroadcastedEvent), evt, "");

            try
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
            catch (Exception ex)
            {
                _log.WriteError(nameof(TransactionBroadcastedEvent), evt, ex);
                throw;
            }
        }

        [UsedImplicitly]
        private async Task Handle(SourceAddressLockReleasedEvent evt, ICommandSender sender)
        {

            _log.WriteInfo(nameof(SourceAddressLockReleasedEvent), evt, "");

            try
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
            catch (Exception ex)
            {
                _log.WriteError(nameof(SourceAddressLockReleasedEvent), evt, ex);
                throw;
            }
        }

        [UsedImplicitly]
        private async Task Handle(OperationExecutionCompletedEvent evt, ICommandSender sender)
        {

            _log.WriteInfo(nameof(OperationExecutionCompletedEvent), evt, "");

            try
            {
                var aggregate = await _repository.GetAsync(evt.OperationId);

                if (aggregate.OnTransactionCompleted(evt.TransactionHash, evt.Block, evt.Fee))
                {
                    sender.SendCommand(new ForgetBroadcastedTransactionCommand
                        {
                            BlockchainType = aggregate.BlockchainType,
                            OperationId = aggregate.OperationId
                        },
                        Self);

                    _chaosKitty.Meow(evt.OperationId);

                    await _repository.SaveAsync(aggregate);
                }
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(OperationExecutionCompletedEvent), evt, ex);
                throw;
            }
        }

        [UsedImplicitly]
        private async Task Handle(OperationExecutionFailedEvent evt, ICommandSender sender)
        {

            _log.WriteInfo(nameof(OperationExecutionFailedEvent), evt, "");

            try
            {
                var aggregate = await _repository.GetAsync(evt.OperationId);

                if (aggregate.OnTransactionFailed(evt.Error))
                {
                    sender.SendCommand(new ForgetBroadcastedTransactionCommand
                        {
                            BlockchainType = aggregate.BlockchainType,
                            OperationId = aggregate.OperationId
                        },
                        Self);

                    _chaosKitty.Meow(evt.OperationId);

                    await _repository.SaveAsync(aggregate);
                }
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(OperationExecutionFailedEvent), evt, ex);
                throw;
            }
        }

        [UsedImplicitly]
        private async Task Handle(BroadcastedTransactionForgottenEvent evt, ICommandSender sender)
        {

            _log.WriteInfo(nameof(BroadcastedTransactionForgottenEvent), evt, "");

            try
            {
                var aggregate = await _repository.GetAsync(evt.OperationId);

                if (aggregate.OnBroadcastedTransactionForgotten())
                {
                    await _repository.SaveAsync(aggregate);
                }
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(BroadcastedTransactionForgottenEvent), evt, ex);
                throw;
            }
        }
    }
}
