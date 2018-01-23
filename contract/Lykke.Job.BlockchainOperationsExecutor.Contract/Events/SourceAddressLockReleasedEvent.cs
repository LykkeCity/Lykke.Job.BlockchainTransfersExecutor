using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Events
{
    /// <summary>
    /// Operation source address lock is released event
    /// </summary>
    [PublicAPI]
    [MessagePackObject]
    public class SourceAddressLockReleasedEvent
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        [Key(0)]
        public Guid OperationId { get; set; }
    }
}
