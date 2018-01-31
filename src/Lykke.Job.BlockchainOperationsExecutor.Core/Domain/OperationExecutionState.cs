namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain
{
    public enum OperationExecutionState
    {
        Started,
        TransactionIsBuilt,
        TransactionIsSigned,
        TransactionIsBroadcasted,
        SourceAddresIsReleased,
        TransactionIsFinished,
        BroadcastedTransactionIsForgotten
    }
}
