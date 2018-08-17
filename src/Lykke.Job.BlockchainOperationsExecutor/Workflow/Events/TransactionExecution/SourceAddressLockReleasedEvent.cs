using System;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution
{
    /// <summary>
    /// Transaction source address lock is released event
    /// </summary>
    [MessagePackObject(keyAsPropertyName: true)]
    public class SourceAddressLockReleasedEvent
    {
        /// <summary>
        /// Lykke unique transaction ID
        /// </summary>
        public Guid TransactionId { get; set; }
    }
}
