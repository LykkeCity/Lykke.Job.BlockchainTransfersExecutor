using System;
using JetBrains.Annotations;
using ProtoBuf;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Events
{
    /// <summary>
    /// Blockchain transaction is broadcasted event
    /// </summary>
    [PublicAPI]
    [ProtoContract]
    public class TransactionBroadcastedEvent
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        [ProtoMember(1)]
        public Guid OperationId { get; set; }
    }

}
