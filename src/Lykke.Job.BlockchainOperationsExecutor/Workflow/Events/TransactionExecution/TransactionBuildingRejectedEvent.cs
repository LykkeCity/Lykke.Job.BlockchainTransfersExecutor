using System;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution
{
    /// <summary>
    /// Blockchain transaction building is rejected
    /// </summary>
    [MessagePackObject(keyAsPropertyName: true)]
    public class TransactionBuildingRejectedEvent
    {
        /// <summary>
        /// Lykke unique transaction ID
        /// </summary>
        public Guid TransactionId { get; set; }
    }
}
