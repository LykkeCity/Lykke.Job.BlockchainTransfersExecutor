using System;
using ProtoBuf;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Events
{
    /// <summary>
    /// Transaction is removed from the list of the broadcasted transactions in the blockchain API 
    /// </summary>
    [ProtoContract]
    public class BroadcastedTransactionForgottenEvent
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        [ProtoMember(1)]
        public Guid OperationId { get; set; }
    }
}
