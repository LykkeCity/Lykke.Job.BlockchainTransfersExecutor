using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Errors;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;
using Lykke.Job.BlockchainOperationsExecutor.Helpers;
using Lykke.Job.BlockchainOperationsExecutor.Services.Transitions.Interfaces;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Sagas
{
    [UsedImplicitly]
    public class OperationExecutionSaga
    {
        private static string Self => BlockchainOperationsExecutorBoundedContext.Name;

        private readonly IChaosKitty _chaosKitty;
        private readonly IOperationExecutionsRepository _repository;
        private readonly ITransitionChecker<OperationExecutionState> _transitionChecker;

        public OperationExecutionSaga(
            IChaosKitty chaosKitty,
            IOperationExecutionsRepository repository,
            ITransitionChecker<OperationExecutionState> transitionChecker)
        {
            _chaosKitty = chaosKitty;
            _repository = repository;
            _transitionChecker = transitionChecker;
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
            
            if (HandleEventTransition(aggregate, evt) 
                && aggregate.OnTransactionBuilt(evt.FromAddressContext, evt.TransactionContext, evt.BlockchainType, evt.BlockchainAssetId))
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

            if (HandleEventTransition(aggregate, evt)
                && aggregate.OnTransactionBuildingRejected())
            {
                sender.SendCommand(new ReleaseSourceAddressLockCommand
                    {
                        BlockchainType = aggregate.BlockchainType,
                        OperationId = aggregate.OperationId,
                        FromAddress = aggregate.FromAddress,
                        BuildingRepeatsIsRequested = false
                    },
                    Self);

                _chaosKitty.Meow(evt.OperationId);

                await _repository.SaveAsync(aggregate);
            }
        }

        [UsedImplicitly]
        private async Task Handle(TransactionReBuildingIsRequestedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (HandleEventTransition(aggregate, evt)
                && aggregate.OnTransactionRebuildingRequested())
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

                _chaosKitty.Meow(evt.OperationId);

                await _repository.SaveAsync(aggregate);
            }
        }

        [UsedImplicitly]
        private async Task Handle(TransactionBuildingFailedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (HandleEventTransition(aggregate, evt)
                && aggregate.OnTransactionBuildingFailed())
            {
                sender.SendCommand(new ReleaseSourceAddressLockCommand
                    {
                        BlockchainType = aggregate.BlockchainType,
                        OperationId = aggregate.OperationId,
                        FromAddress = aggregate.FromAddress,
                        BuildingRepeatsIsRequested = false,
                        OperationExecutionErrorCode = evt.ErrorCode.MapToOperationExecutionErrorCode()
                    },
                    Self);

                _chaosKitty.Meow(evt.OperationId);

                await _repository.SaveAsync(aggregate);
            }
        }

        [UsedImplicitly]
        private async Task Handle(TransactionBroadcastingFailedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (HandleEventTransition(aggregate, evt)
                && aggregate.OnTransactionBuildingRejected())
            {
                sender.SendCommand(new ReleaseSourceAddressLockCommand
                    {
                        BlockchainType = aggregate.BlockchainType,
                        OperationId = aggregate.OperationId,
                        FromAddress = aggregate.FromAddress,
                        BuildingRepeatsIsRequested = false,
                        OperationExecutionErrorCode = evt.ErrorCode.MapToOperationExecutionErrorCode()
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

            if (HandleEventTransition(aggregate, evt)
                && aggregate.OnTransactionSigned(evt.SignedTransaction))
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

            if (HandleEventTransition(aggregate, evt)
                && aggregate.OnTransactionBroadcasted())
            {
                sender.SendCommand(new ReleaseSourceAddressLockCommand
                    {
                        BlockchainType = aggregate.BlockchainType,
                        FromAddress = aggregate.FromAddress,
                        OperationId = aggregate.OperationId,
                        BuildingRepeatsIsRequested = false
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

            if (HandleEventTransition(aggregate, evt)
                && aggregate.OnSourceAddressLockReleased())
            {
                sender.SendCommand(new WaitForTransactionEndingCommand
                    {
                        BlockchainType = aggregate.BlockchainType,
                        BlockchainAssetId = aggregate.BlockchainAssetId,
                        OperationId = aggregate.OperationId,
                        ErrorCode = evt.OperationExecutionErrorCode
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

            if (HandleEventTransition(aggregate, evt)
                && aggregate.OnTransactionCompleted(evt.TransactionHash, evt.Block, evt.Fee))
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

        [UsedImplicitly]
        private async Task Handle(OperationExecutionFailedEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (HandleEventTransition(aggregate, evt)
                && aggregate.OnTransactionFailed(evt.Error))
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

        [UsedImplicitly]
        private async Task Handle(BroadcastedTransactionForgottenEvent evt, ICommandSender sender)
        {
            var aggregate = await _repository.GetAsync(evt.OperationId);

            if (HandleEventTransition(aggregate, evt)
                && aggregate.OnBroadcastedTransactionForgotten())
            {
                await _repository.SaveAsync(aggregate);
            }
        }

        private bool HandleEventTransition(OperationExecutionAggregate aggregate, object @event)
        {
            var checkTranstionResult = _transitionChecker.CheckTransition(aggregate.State, @event);

            if (checkTranstionResult.IsValid)
            {
                aggregate.State = checkTranstionResult.NextState;
            }

            return checkTranstionResult.IsValid;
        }
    }
}
