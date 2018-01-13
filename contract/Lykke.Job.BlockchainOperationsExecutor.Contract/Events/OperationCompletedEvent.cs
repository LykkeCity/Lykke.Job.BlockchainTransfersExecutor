using JetBrains.Annotations;
using ProtoBuf;

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
        [ProtoMember(3)]
        public string TransactionHash { get; set; }

        /// <summary>
        /// Actual fee of the operation
        /// </summary>
        [ProtoMember(4)]
        public decimal Fee { get; set; }
    }
}
