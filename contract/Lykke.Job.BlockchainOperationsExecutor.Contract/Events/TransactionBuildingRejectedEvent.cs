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
        [Key(0)]
        public Guid OperationId { get;set; }
    }
}