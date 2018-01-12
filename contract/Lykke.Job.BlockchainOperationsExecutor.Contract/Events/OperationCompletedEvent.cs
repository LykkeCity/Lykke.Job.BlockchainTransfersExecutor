using JetBrains.Annotations;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Events
{
    /// <summary>
    /// Operation is completed
    /// </summary>
    [PublicAPI]
    public class OperationCompletedEvent : BaseOperationFinishingEvent
    {
        /// <summary>
        /// Hash of the blockchain transaction
        /// </summary>
        public string Hash { get; set; }
    }
}
