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
        [ProtoMember(9)]
        public string Hash { get; set; }
    }
}
