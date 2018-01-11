using JetBrains.Annotations;

namespace Lykke.Job.BlockchainTransfersExecutor.Contract.Events
{
    /// <summary>
    /// Transfer is completed
    /// </summary>
    [PublicAPI]
    public class TransferCompletedEvent : BaseTransferFinishingEvent
    {
        /// <summary>
        /// Hash of the blockchain transaction
        /// </summary>
        public string Hash { get; set; }
    }
}
