using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Events
{
    /// <summary>
    /// Blockchain transaction building is rejected
    /// </summary>
    [PublicAPI]
    [MessagePackObject]
    public class TransactionBuildingRejectedEvent
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        [Key(0)]
        public Guid OperationId { get;set; }
    }
}
