using System;

namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain
{
    public class InvalidAggregateStateException : Exception
    {
        public InvalidAggregateStateException(OperationExecutionState currentState, OperationExecutionState expectedState, OperationExecutionState targetState) :
            base(BuildMessage(currentState, expectedState, targetState))
        {

        }

        private static string BuildMessage(OperationExecutionState currentState, OperationExecutionState expectedState, OperationExecutionState targetState)
        {
            return $"Operation execution state can't be switched: {currentState} -> {targetState}. Waiting for the {expectedState} state.";
        }
    }
}
