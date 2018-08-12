namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain
{
    public enum TransactionExecutionState
    {
        Started,
        Built,
        Signed,
        Broadcasted,
        SourceAddressReleased,
        Completed,
        Failed,
        Cleared
    }
}
