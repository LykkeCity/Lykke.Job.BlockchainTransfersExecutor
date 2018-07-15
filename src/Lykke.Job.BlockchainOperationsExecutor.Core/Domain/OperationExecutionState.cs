namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain
{
    public enum OperationExecutionState
    {
        Started,
        TransactionIsBuilt,
        TransactionBuildingFailed,
        TransactionBroadcastingFailed,
        TransactionIsSigned,
        TransactionIsBroadcasted,
        SourceAddresIsReleased,
        TransactionIsFinished,
        BroadcastedTransactionIsForgotten
    }
}
