using System;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution
{
    /// <summary>
    /// Blockchain transaction is broadcasted event
    /// </summary>
    [MessagePackObject(keyAsPropertyName: true)]
    public class TransactionBroadcastedEvent
    {
        /// <summary>
        /// Transaction ID, which is broadcasted
        /// </summary>
        public Guid TransactionId { get; set; }
    }

}
