using JetBrains.Annotations;

namespace Lykke.Job.BlockchainTransfersExecutor.Contract.Events
{
    /// <summary>
    /// Transfer is failed
    /// </summary>
    [PublicAPI]
    public class TransferFailedEvent : BaseTransferFinishingEvent
    {
        /// <summary>
        /// Error description
        /// </summary>
        public string Error { get; set; }
    }
}
