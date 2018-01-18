using System;
using ProtoBuf;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Events
{
    /// <summary>
    /// Blockchain transaction is signed event
    /// </summary>
    [ProtoContract]
    public class TransactionSignedEvent
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        [ProtoMember(1)]
        public Guid OperationId { get; set; }

        /// <summary>
        /// Signed transaction
        /// </summary>
        [ProtoMember(2)]
        public string SignedTransaction { get; set; }
    }
}
