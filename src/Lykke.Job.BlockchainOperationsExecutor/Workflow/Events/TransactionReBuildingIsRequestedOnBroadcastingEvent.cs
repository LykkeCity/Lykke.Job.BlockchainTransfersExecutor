using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Events
{    
    /// <summary>
    /// Blockchain transaction building repeats is requested on broadcasting
    /// </summary>
    [PublicAPI]
    [MessagePackObject]
    public class TransactionReBuildingIsRequestedOnBroadcastingEvent
    {        
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        [Key(0)]
        public Guid OperationId { get; set; }

        /// <summary>
        /// Transaction ID which should be rebuilt
        /// </summary>
        [Key(1)]
        public Guid TransactionId { get; set; }
    }
}
