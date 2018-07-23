namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain
{
    public enum OperationExecutionState
    {
        Started,
        TransactionIsBuilt,
        TransactionBuildingFailed,
        TransactionBroadcastingFailed,
        TransactionIsSigned,
        TransactionRebuildingRequestedOnBroadcasting,
        TransactionIsBroadcasted,
        SourceAddresIsReleased,
        TransactionIsFinished,
        BroadcastedTransactionIsForgotten
    }
}
