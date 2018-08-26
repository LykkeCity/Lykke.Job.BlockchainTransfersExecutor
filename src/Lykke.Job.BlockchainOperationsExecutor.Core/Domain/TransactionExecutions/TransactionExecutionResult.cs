namespace Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions
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
