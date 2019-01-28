using System.Linq;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Mappers;
using Lykke.Job.BlockchainOperationsExecutor.StateMachine.Building;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution;

namespace Lykke.Job.BlockchainOperationsExecutor.StateMachine
{
    public static class TransitionExecutionStateSwitcherBuilder
    {
        public static IStateSwitcher<TransactionExecutionAggregate> Build()
        {
            var register = TransitionRegisterFactory.StartRegistrationFor<TransactionExecutionAggregate, TransactionExecutionState>();

            register.GetCurrentStateWith(aggregate => aggregate.State);

            register.From(TransactionExecutionState.Started, outputs =>
            {
                outputs.On<SourceAddressLockedEvent>()
                    .HandleTransition((a, e) => a.OnSourceAddressLocked());

                outputs.On<SourceAndTargetAddressesLockedEvent>()
                    .HandleTransition((a, e) => a.OnSourceAndTargetAddressesLocked());
            });

            register.From(TransactionExecutionState.SourceAddressLocked, outputs =>
            {
                outputs.On<TransactionBuiltEvent>()
                    .HandleTransition((a, e) => a.OnBuilt(e.FromAddressContext, e.TransactionContext));

                outputs.On<TransactionExecutionFailedEvent>()
                    .HandleTransition((a, e) => a.OnBuildingFailed(e.ErrorCode, e.Error));
            });

            register.From(TransactionExecutionState.Built)
                .On<TransactionSignedEvent>()
                .HandleTransition((a, e) => a.OnSigned(e.SignedTransaction));

            register.From(TransactionExecutionState.Signed, outputs =>
            {
                outputs.On<TransactionBroadcastedEvent>()
                    .HandleTransition((a, e) => a.OnBroadcasted());

                outputs.On<TransactionExecutionFailedEvent>()
                    .HandleTransition((a, e) => a.OnBroadcastingFailed(e.ErrorCode, e.Error));

                outputs.On<TransactionExecutionRepeatRequestedEvent>()
                    .HandleTransition((a, e) => a.OnBroadcastingFailed(e.ErrorCode, e.Error));
            });

            register.From(TransactionExecutionState.Broadcasted)
                .On<SourceAddressLockReleasedEvent>()
                .HandleTransition((a, e) => a.OnWaitingForEndingStarted());

            register.From(TransactionExecutionState.WaitingForEnding, outputs =>
            {
                outputs.On<TransactionExecutionCompletedEvent>()
                    .HandleTransition((a, e) => a.OnCompleted(
                        e.TransactionOutputs?
                            .Select(o => o.ToDomain())
                            .ToArray(),
                        e.TransactionBlock,
                        e.TransactionFee,
                        e.TransactionHash));

                outputs.On<TransactionExecutionFailedEvent>()
                    .HandleTransition((a, e) => a.OnWaitingForEndingFailed(e.ErrorCode, e.Error));

                outputs.On<TransactionExecutionRepeatRequestedEvent>()
                    .HandleTransition((a, e) => a.OnWaitingForEndingFailed(e.ErrorCode, e.Error));
            });

            register.From(TransactionExecutionState.Completed, outputs =>
            {
                outputs.On<BroadcastedTransactionClearedEvent>()
                    .HandleTransition((a, e) => a.OnCleared());
                
                outputs.On<SourceAndTargetAddressLocksReleasedEvent>()
                    .HandleTransition((a, e) => a.OnSourceAndTargetAddressLocksReleased());
            });

            register.From(TransactionExecutionState.SourceAndTargetAddressesReleased)
                .On<BroadcastedTransactionClearedEvent>()
                .HandleTransition((a, e) => a.OnCleared());
            
            register.From(TransactionExecutionState.WaitingForEndingFailed)
                .On<BroadcastedTransactionClearedEvent>()
                .HandleTransition((a, e) => a.OnCleared());

            register.From(TransactionExecutionState.BuildingFailed, outputs =>
            {
                outputs.On<SourceAddressLockReleasedEvent>()
                    .HandleTransition((a, e) => a.OnSourceAddressLockReleased());
                
                outputs.On<SourceAndTargetAddressLocksReleasedEvent>()
                    .HandleTransition((a, e) => a.OnSourceAndTargetAddressLocksReleased());
            });

            register.From(TransactionExecutionState.BroadcastingFailed, outputs =>
            {
                outputs.On<SourceAddressLockReleasedEvent>()
                    .HandleTransition((a, e) => a.OnSourceAddressLockReleased());
                
                outputs.On<SourceAndTargetAddressLocksReleasedEvent>()
                    .HandleTransition((a, e) => a.OnSourceAndTargetAddressLocksReleased());
            });

            register.From(TransactionExecutionState.SourceAddressReleased)
                .On<BroadcastedTransactionClearedEvent>()
                .HandleTransition((a, e) => a.OnCleared());

            // Ignore events which already processed and possibly could be retried due to infrastructure failures

            register.In(TransactionExecutionState.Started)
                .Ignore<TransactionExecutionStartedEvent>();

            register.In(TransactionExecutionState.SourceAddressLocked)
                .Ignore<TransactionExecutionStartedEvent>()
                .Ignore<SourceAddressLockedEvent>();

            register.In(TransactionExecutionState.SourceAndTargetAddressesLocked)
                .Ignore<TransactionExecutionStartedEvent>()
                .Ignore<SourceAndTargetAddressesLockedEvent>();
            
            register.In(TransactionExecutionState.Built)
                .Ignore<TransactionExecutionStartedEvent>()
                .Ignore<SourceAddressLockedEvent>()
                .Ignore<TransactionBuiltEvent>();

            register.In(TransactionExecutionState.BuildingFailed)
                .Ignore<TransactionExecutionStartedEvent>()
                .Ignore<SourceAddressLockedEvent>()
                .Ignore<TransactionExecutionFailedEvent>();

            register.In(TransactionExecutionState.Signed)
                .Ignore<TransactionExecutionStartedEvent>()
                .Ignore<SourceAddressLockedEvent>()
                .Ignore<TransactionBuiltEvent>()
                .Ignore<TransactionSignedEvent>();

            register.In(TransactionExecutionState.Broadcasted)
                .Ignore<TransactionExecutionStartedEvent>()
                .Ignore<SourceAddressLockedEvent>()
                .Ignore<TransactionBuiltEvent>()
                .Ignore<TransactionSignedEvent>()
                .Ignore<TransactionBroadcastedEvent>();

            register.In(TransactionExecutionState.BroadcastingFailed)
                .Ignore<TransactionExecutionStartedEvent>()
                .Ignore<SourceAddressLockedEvent>()
                .Ignore<TransactionBuiltEvent>()
                .Ignore<TransactionSignedEvent>()
                .Ignore<TransactionExecutionFailedEvent>()
                .Ignore<TransactionExecutionRepeatRequestedEvent>();

            register.In(TransactionExecutionState.WaitingForEnding)
                .Ignore<TransactionExecutionStartedEvent>()
                .Ignore<SourceAddressLockedEvent>()
                .Ignore<TransactionBuiltEvent>()
                .Ignore<TransactionSignedEvent>()
                .Ignore<TransactionBroadcastedEvent>()
                .Ignore<SourceAddressLockReleasedEvent>();

            register.In(TransactionExecutionState.SourceAddressReleased)
                .Ignore<TransactionExecutionStartedEvent>()
                .Ignore<SourceAddressLockedEvent>()
                .Ignore<TransactionBuiltEvent>()
                .Ignore<TransactionSignedEvent>()
                .Ignore<TransactionExecutionFailedEvent>()
                .Ignore<TransactionExecutionRepeatRequestedEvent>()
                .Ignore<SourceAddressLockReleasedEvent>();

            register.In(TransactionExecutionState.Completed)
                .Ignore<TransactionExecutionStartedEvent>()
                .Ignore<SourceAddressLockedEvent>()
                .Ignore<TransactionBuiltEvent>()
                .Ignore<TransactionSignedEvent>()
                .Ignore<TransactionBroadcastedEvent>()
                .Ignore<SourceAddressLockReleasedEvent>()
                .Ignore<TransactionExecutionCompletedEvent>();

            register.In(TransactionExecutionState.WaitingForEndingFailed)
                .Ignore<TransactionExecutionStartedEvent>()
                .Ignore<SourceAddressLockedEvent>()
                .Ignore<TransactionBuiltEvent>()
                .Ignore<TransactionSignedEvent>()
                .Ignore<TransactionBroadcastedEvent>()
                .Ignore<SourceAddressLockReleasedEvent>()
                .Ignore<TransactionExecutionFailedEvent>()
                .Ignore<TransactionExecutionRepeatRequestedEvent>();          

            register.In(TransactionExecutionState.SourceAndTargetAddressesReleased)
                .Ignore<TransactionExecutionStartedEvent>()
                .Ignore<SourceAddressLockedEvent>()
                .Ignore<SourceAndTargetAddressesLockedEvent>()
                .Ignore<TransactionBuiltEvent>()
                .Ignore<TransactionSignedEvent>()
                .Ignore<TransactionBroadcastedEvent>()
                .Ignore<SourceAddressLockReleasedEvent>()
                .Ignore<SourceAndTargetAddressLocksReleasedEvent>()
                .Ignore<TransactionExecutionFailedEvent>()
                .Ignore<TransactionExecutionRepeatRequestedEvent>();
            
            register.In(TransactionExecutionState.Cleared)
                .Ignore<TransactionExecutionStartedEvent>()
                .Ignore<SourceAddressLockedEvent>()
                .Ignore<TransactionBuiltEvent>()
                .Ignore<TransactionSignedEvent>()
                .Ignore<TransactionBroadcastedEvent>()
                .Ignore<SourceAddressLockReleasedEvent>()
                .Ignore<TransactionExecutionCompletedEvent>()
                .Ignore<TransactionExecutionFailedEvent>()
                .Ignore<TransactionExecutionRepeatRequestedEvent>()
                .Ignore<BroadcastedTransactionClearedEvent>();

            return register.Build();
        }
    }
}
