using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.OperationExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
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
                .WithPrecondition((a, e) => e.TransactionNumber == 1, (a, e) => "Transaction number should be 1")
                .WithPrecondition((a, e) => a.ActiveTransactionNumber == 0, (a, e) => "Active transaction number should be 0")
                .WithPrecondition((a, e) => a.ActiveTransactionId == null, (a, e) => "Active transaction should be null")
                .HandleTransition((a, e) => a.OnActiveTransactionIdGenerated(e.TransactionId, e.TransactionNumber));

            register.From(OperationExecutionState.ActiveTransactionIdGenerated)
                .On<TransactionExecutionStartedEvent>()
                .WithPrecondition((a, e) => a.ActiveTransactionNumber == e.TransactionNumber, (a, e) => $"Unexpected transaction number. Active transaction number is [{a.ActiveTransactionNumber}]")
                .HandleTransition((a, e) => a.OnTransactionExecutionStarted());

            register.From(OperationExecutionState.TransactionExecutionInProgress, outputs =>
            {
                outputs.On<TransactionExecutionRepeatRequestedEvent>()
                    .WithPrecondition((a, e) => a.ActiveTransactionNumber == e.TransactionNumber, (a, e) => $"Unexpected transaction number. Active transaction number is [{a.ActiveTransactionNumber}]")
                    .HandleTransition((a, e) => a.OnTransactionExecutionRepeatRequested(
                        e.Error));

                outputs.On<TransactionExecutionCompletedEvent>()
                    .WithPrecondition((a, e) => a.ActiveTransactionNumber == e.TransactionNumber, (a, e) => $"Unexpected transaction number. Active transaction number is [{a.ActiveTransactionNumber}]")
                    .HandleTransition((a, e) => a.OnTransactionExecutionCompleted(
                        e.TransactionAmount,
                        e.TransactionBlock,
                        e.TransactionFee,
                        e.TransactionHash));

                outputs.On<TransactionExecutionFailedEvent>()
                    .WithPrecondition((a, e) => a.ActiveTransactionNumber == e.TransactionNumber, (a, e) => $"Unexpected transaction number. Active transaction number is [{a.ActiveTransactionNumber}]")
                    .WithPrecondition((a, e) => e.ErrorCode != TransactionExecutionResult.Completed, (a, e) => $"Error code should be not {TransactionExecutionResult.Completed}")
                    .HandleTransition((a, e) => a.OnTransactionExecutionFailed(
                        e.TransactionNumber,
                        e.ErrorCode.MapToOperationExecutionResult(),
                        e.Error));
            });

            register.From(OperationExecutionState.TransactionExecutionRepeatRequested)
                .On<ActiveTransactionClearedEvent>()
                .WithPrecondition((a, e) => a.ActiveTransactionNumber == e.TransactionNumber, (a, e) => $"Unexpected transaction number. Active transaction number is [{a.ActiveTransactionNumber}]")
                .WithPrecondition((a, e) => a.ActiveTransactionId != null, (a, e) => "Active transaction should be not null")
                .HandleTransition((a, e) => a.OnActiveTransactionCleared());

            register.From(OperationExecutionState.ActiveTransactionCleared)
                .On<ActiveTransactionIdGeneratedEvent>()
                .WithPrecondition((a, e) => e.TransactionNumber == a.ActiveTransactionNumber + 1, (a, e) => $"Transaction number should be active transaction number [{a.ActiveTransactionNumber}] + 1")
                .WithPrecondition((a, e) => a.ActiveTransactionId == null, (a, e) => "Active transaction should be null")
                .HandleTransition((a, e) => a.OnActiveTransactionIdGenerated(e.TransactionId, e.TransactionNumber));

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
                .Ignore<ActiveTransactionIdGeneratedEvent>((a, e) => a.ActiveTransactionNumber >= e.TransactionNumber)
                .Ignore<TransactionExecutionStartedEvent>((a, e) => a.ActiveTransactionNumber > e.TransactionNumber)
                .Ignore<TransactionExecutionRepeatRequestedEvent>((a, e) => a.ActiveTransactionNumber > e.TransactionNumber)
                .Ignore<ActiveTransactionClearedEvent>((a, e) => a.ActiveTransactionNumber > e.TransactionNumber);

            register.In(OperationExecutionState.TransactionExecutionInProgress)
                .Ignore<OperationExecutionStartedEvent>()
                .Ignore<ActiveTransactionIdGeneratedEvent>((a, e) => a.ActiveTransactionNumber >= e.TransactionNumber)
                .Ignore<TransactionExecutionStartedEvent>((a, e) => a.ActiveTransactionNumber >= e.TransactionNumber)
                .Ignore<TransactionExecutionRepeatRequestedEvent>((a, e) => a.ActiveTransactionNumber > e.TransactionNumber)
                .Ignore<ActiveTransactionClearedEvent>((a, e) => a.ActiveTransactionNumber > e.TransactionNumber);

            register.In(OperationExecutionState.TransactionExecutionRepeatRequested)
                .Ignore<OperationExecutionStartedEvent>()
                .Ignore<ActiveTransactionIdGeneratedEvent>((a, e) => a.ActiveTransactionNumber >= e.TransactionNumber)
                .Ignore<TransactionExecutionStartedEvent>((a, e) => a.ActiveTransactionNumber >= e.TransactionNumber)
                .Ignore<TransactionExecutionRepeatRequestedEvent>((a, e) => a.ActiveTransactionNumber >= e.TransactionNumber)
                .Ignore<ActiveTransactionClearedEvent>((a, e) => a.ActiveTransactionNumber > e.TransactionNumber);

            register.In(OperationExecutionState.ActiveTransactionCleared)
                .Ignore<OperationExecutionStartedEvent>()
                .Ignore<ActiveTransactionIdGeneratedEvent>((a, e) => a.ActiveTransactionNumber >= e.TransactionNumber)
                .Ignore<TransactionExecutionStartedEvent>((a, e) => a.ActiveTransactionNumber >= e.TransactionNumber)
                .Ignore<TransactionExecutionRepeatRequestedEvent>((a, e) => a.ActiveTransactionNumber >= e.TransactionNumber)
                .Ignore<ActiveTransactionClearedEvent>((a, e) => a.ActiveTransactionNumber >= e.TransactionNumber);

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
