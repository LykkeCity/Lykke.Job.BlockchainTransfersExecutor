namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain
{
    public enum OperationExecutionState
    {
        Started,
        TransactionIsBuilt,
        TransactionIsSigned,
        TransactionIsBroadcasted,
        TransactionIsCompleted,
        TransactionIsFailed,
        Completed,
        Failed
    }
}
