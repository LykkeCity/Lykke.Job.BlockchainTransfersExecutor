using JetBrains.Annotations;

namespace Lykke.Job.BlockchainTransfersExecutor.Contract
{
    /// <summary>
    /// Generic blockchain integration transfer executor bounded context constants
    /// </summary>
    [PublicAPI]
    public static class BlockchainTransferExecutorBoundedContext
    {
        /// <summary>
        /// Context name
        /// </summary>
        public static string Name = "transfers-executor";
    }
}
