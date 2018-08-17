using System;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution
{
    /// <summary>
    /// Transaction is removed from the list of the broadcasted transactions in the blockchain API 
    /// </summary>
    [MessagePackObject(keyAsPropertyName: true)]
    public class BroadcastedTransactionClearedEvent
    {
        /// <summary>
        /// Lykke unique transaction ID
        /// </summary>
        public Guid TransactionId { get; set; }
    }
}
