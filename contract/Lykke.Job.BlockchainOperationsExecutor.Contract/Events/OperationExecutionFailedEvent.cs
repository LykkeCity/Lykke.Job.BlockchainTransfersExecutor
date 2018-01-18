using JetBrains.Annotations;
using ProtoBuf;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Events
{
    /// <summary>
    /// Operation execution is failed
    /// </summary>
    [PublicAPI]
    public class OperationExecutionFailedEvent : BaseOperationFinishingEvent
    {
        /// <summary>
        /// Error description
        /// </summary>
        [ProtoMember(2)]
        public string Error { get; set; }
    }
}
