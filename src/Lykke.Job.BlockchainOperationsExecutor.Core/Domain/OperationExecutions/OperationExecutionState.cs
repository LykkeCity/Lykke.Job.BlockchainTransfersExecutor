namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain.OperationExecutions
{
    public enum OperationExecutionState
    {
        Started,
        ActiveTransactionIdGenerated,
        TransactionExecutionInProgress,
        TransactionExecutionRepeatRequested,
        ActiveTransactionCleared,
        Completed,
        Failed,
        NotifiedAboutEnding
    }
}
