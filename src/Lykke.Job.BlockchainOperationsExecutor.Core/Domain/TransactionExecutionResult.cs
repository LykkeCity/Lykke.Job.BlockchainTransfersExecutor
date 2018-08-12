namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain
{
    public enum TransactionExecutionResult
    {
        Completed,
        UnknownError,
        AmountIsTooSmall,
        NotEnoughBalance,
        RebuildingIsRequired
    }
}
