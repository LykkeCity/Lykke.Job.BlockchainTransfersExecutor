using System;

namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain
{
    public class InvalidAggregateStateException : Exception
    {
        public InvalidAggregateStateException(TransactionExecutionState currentState, TransactionExecutionState expectedState, TransactionExecutionState targetState) :
            base(BuildMessage(currentState, expectedState, targetState))
        {

        }

        private static string BuildMessage(TransactionExecutionState currentState, TransactionExecutionState expectedState, TransactionExecutionState targetState)
        {
            return $"Operation execution state can't be switched: {currentState} -> {targetState}. Waiting for the {expectedState} state.";
        }
    }
}
