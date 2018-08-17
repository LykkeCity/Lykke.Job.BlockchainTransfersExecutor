namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions
{
    public enum TransactionExecutionState
    {
        Started,
        SourceAddressLocked,
        Built,
        BuildingFailed,
        Signed,
        Broadcasted,
        BroadcastingFailed,
        WaitingForEnding,
        WaitingForEndingFailed,
        SourceAddressReleased,
        Completed,
        Cleared
    }
}
