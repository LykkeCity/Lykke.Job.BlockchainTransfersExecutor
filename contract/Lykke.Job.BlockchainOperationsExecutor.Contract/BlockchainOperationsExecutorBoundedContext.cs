using JetBrains.Annotations;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract
{
    /// <summary>
    /// Generic blockchain integration operations executor bounded context constants
    /// </summary>
    [PublicAPI]
    public static class BlockchainOperationsExecutorBoundedContext
    {
        /// <summary>
        /// Context name
        /// </summary>
        public static string Name = "bcn-integration.operations-executor";
    }
}
