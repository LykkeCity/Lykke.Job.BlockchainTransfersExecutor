using JetBrains.Annotations;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract
{
    /// <summary>
    /// Provides information about reason of operation failure
    /// </summary>
    [PublicAPI]
    public enum OperationExecutionErrorCode
    {
        Unknown,
        AmountTooSmall,
        RebuildingRejected
    }
}
