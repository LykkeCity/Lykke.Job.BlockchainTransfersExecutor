using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Events
{    
    /// <summary>
    /// Blockchain transaction building repeats is requested
    /// </summary>
    [PublicAPI]
    [MessagePackObject]
    public class TransactionReBuildingIsRequestedEvent
    {        
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        [Key(0)]
        public Guid OperationId { get; set; }
    }
}
