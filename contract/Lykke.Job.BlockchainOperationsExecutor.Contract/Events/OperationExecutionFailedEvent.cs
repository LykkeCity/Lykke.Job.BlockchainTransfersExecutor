using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Events
{
    /// <summary>
    /// Operation execution is failed
    /// </summary>
    [PublicAPI]
    [MessagePackObject]
    public class OperationExecutionFailedEvent
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        [Key(0)]
        public Guid OperationId { get; set; }

        /// <summary>
        /// Error description
        /// </summary>
        [Key(1)]
        public string Error { get; set; }
    }
}
