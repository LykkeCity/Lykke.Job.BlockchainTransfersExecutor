using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.OperationExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Mappers;
using Lykke.Job.BlockchainOperationsExecutor.StateMachine.Building;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.OperationExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution;

namespace Lykke.Job.BlockchainOperationsExecutor.StateMachine
{
    public static class OperationExecutionStateSwitcherBuilder
    {
        public static IStateSwitcher<OperationExecutionAggregate> Build()
        {
            var register = TransitionRegisterFactory.StartRegistrationFor<OperationExecutionAggregate, OperationExecutionState>();

            register.GetCurrentStateWith(aggregate => aggregate.State);

            register.From(OperationExecutionState.Started)
                .On<ActiveTransactionIdGeneratedEvent>()
                .HandleTransition((a, e) => a.OnActiveTransactionIdGenerated(e.TransactionId));

            register.From(OperationExecutionState.ActiveTransactionIdGenerated)
                .On<TransactionExecutionStartedEvent>()
                .HandleTransition((a, e) => a.OnTransactionExecutionStarted());

            register.From(OperationExecutionState.TransactionExecutionInProgress, outputs =>
            {
                outputs.On<TransactionExecutionRepeatRequestedEvent>()
                    .HandleTransition((a, e) => a.OnTransactionExecutionRepeatRequested(
                        e.TransactionId,
                        e.Error));

                outputs.On<TransactionExecutionCompletedEvent>()
                    .HandleTransition((a, e) => a.OnTransactionExecutionCompleted(
                        e.TransactionId,
                        e.TransactionAmount,
                        e.TransactionBlock,
                        e.TransactionFee,
                        e.TransactionHash));

                outputs.On<TransactionExecutionFailedEvent>()
                    .HandleTransition((a, e) => a.OnTransactionExecutionFailed(
                        e.TransactionId,
                        e.ErrorCode.MapToOperationExecutionResult(),
                        e.Error));
            });

            register.From(OperationExecutionState.TransactionExecutionRepeatRequested)
                .On<ActiveTransactionClearedEvent>()
                .HandleTransition((a, e) => a.OnActiveTransactionCleared());

            register.From(OperationExecutionState.ActiveTransactionCleared)
                .On<ActiveTransactionIdGeneratedEvent>()
                .HandleTransition((a, e) => a.OnActiveTransactionIdGenerated(e.TransactionId));

            register.From(OperationExecutionState.Completed)
                .On<OperationExecutionCompletedEvent>()
                .HandleTransition((a, e) => a.OnNotifiedAboutEnding());

            register.From(OperationExecutionState.Failed)
                .On<OperationExecutionFailedEvent>()
                .HandleTransition((a, e) => a.OnNotifiedAboutEnding());

            // Ignore events which already processed and possibly could be retried due to infrastructure failures

            register.In(OperationExecutionState.Started)
                .Ignore<OperationExecutionStartedEvent>();

            register.In(OperationExecutionState.ActiveTransactionIdGenerated)
                .Ignore<OperationExecutionStartedEvent>()
                .Ignore<ActiveTransactionIdGeneratedEvent>()
                .Ignore<TransactionExecutionRepeatRequestedEvent>()
                .Ignore<ActiveTransactionClearedEvent>();

            register.In(OperationExecutionState.TransactionExecutionInProgress)
                .Ignore<OperationExecutionStartedEvent>()
                .Ignore<ActiveTransactionIdGeneratedEvent>()
                .Ignore<TransactionExecutionStartedEvent>()
                .Ignore<ActiveTransactionClearedEvent>();

            register.In(OperationExecutionState.TransactionExecutionRepeatRequested)
                .Ignore<OperationExecutionStartedEvent>()
                .Ignore<ActiveTransactionIdGeneratedEvent>()
                .Ignore<TransactionExecutionStartedEvent>();

            register.In(OperationExecutionState.ActiveTransactionCleared)
                .Ignore<OperationExecutionStartedEvent>()
                .Ignore<TransactionExecutionStartedEvent>()
                .Ignore<TransactionExecutionRepeatRequestedEvent>()
                .Ignore<ActiveTransactionClearedEvent>();

            register.In(OperationExecutionState.Completed)
                .Ignore<OperationExecutionStartedEvent>()
                .Ignore<ActiveTransactionIdGeneratedEvent>()
                .Ignore<TransactionExecutionStartedEvent>()
                .Ignore<TransactionExecutionRepeatRequestedEvent>()
                .Ignore<ActiveTransactionClearedEvent>()
                .Ignore<TransactionExecutionCompletedEvent>();

            register.In(OperationExecutionState.Failed)
                .Ignore<OperationExecutionStartedEvent>()
                .Ignore<ActiveTransactionIdGeneratedEvent>()
                .Ignore<TransactionExecutionStartedEvent>()
                .Ignore<TransactionExecutionRepeatRequestedEvent>()
                .Ignore<ActiveTransactionClearedEvent>()
                .Ignore<TransactionExecutionFailedEvent>();

            register.In(OperationExecutionState.NotifiedAboutEnding)
                .Ignore<OperationExecutionStartedEvent>()
                .Ignore<ActiveTransactionIdGeneratedEvent>()
                .Ignore<TransactionExecutionStartedEvent>()
                .Ignore<TransactionExecutionRepeatRequestedEvent>()
                .Ignore<ActiveTransactionClearedEvent>()
                .Ignore<TransactionExecutionCompletedEvent>()
                .Ignore<TransactionExecutionFailedEvent>()
                .Ignore<OperationExecutionCompletedEvent>()
                .Ignore<OperationExecutionFailedEvent>();

            return register.Build();
        }
    }
}
