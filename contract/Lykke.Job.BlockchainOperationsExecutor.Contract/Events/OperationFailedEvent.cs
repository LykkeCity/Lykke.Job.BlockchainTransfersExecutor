using JetBrains.Annotations;
using ProtoBuf;

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
        [ProtoMember(9)]
        public string Error { get; set; }
    }
}
