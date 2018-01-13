using System;
using JetBrains.Annotations;
using ProtoBuf;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Events
{
    /// <summary>
    /// Base class for the operation finishing events
    /// </summary>
    [PublicAPI]
    [ProtoContract]
    public abstract class BaseOperationFinishingEvent
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        [ProtoMember(1)]
        public Guid OperationId { get; set; }

        /// <summary>
        /// Blockchain transaction moment
        /// </summary>
        [ProtoMember(2)]
        public DateTime TransactionTimestamp { get; set; }
    }
}
