using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Events
{
    /// <summary>
    /// Blockchain transaction is broadcasted event
    /// </summary>
    [PublicAPI]
    [MessagePackObject]
    public class TransactionBroadcastedEvent
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        [Key(0)]
        public Guid OperationId { get; set; }


        /// <summary>
        /// Transaction, which is broadcasted
        /// </summary>
        [Key(1)]
        public Guid TransactionId { get; set; }
    }

}
