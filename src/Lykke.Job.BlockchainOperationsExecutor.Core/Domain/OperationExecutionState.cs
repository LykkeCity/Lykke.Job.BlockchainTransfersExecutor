namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain
{
    public enum OperationExecutionState
    {
        Started,
        TransactionIsBuilt,
        TransactionIsSigned,
        TransactionIsBroadcasted,
        TransactionIsFinished,
        SourceAddresIsReleased,
        BroadcastedTransactionIsForgotten
    }
}
