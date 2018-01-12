using JetBrains.Annotations;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Events
{
    /// <summary>
    /// Operation is failed
    /// </summary>
    [PublicAPI]
    public class OperationFailedEvent : BaseOperationFinishingEvent
    {
        /// <summary>
        /// Error description
        /// </summary>
        public string Error { get; set; }
    }
}
