using System;
using JetBrains.Annotations;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Errors;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Events
{    
    /// <summary>
    /// Blockchain transaction building failed event
    /// </summary>
    [PublicAPI]
    [MessagePackObject]
    public class TransactionBuildingFailedEvent
    {        
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        [Key(0)]
        public Guid OperationId { get; set; }

        /// <summary>
        /// Error code
        /// </summary>
        [Key(1)]
        public TransactionBuildingErrorCode ErrorCode { get; set; }
    }
}
