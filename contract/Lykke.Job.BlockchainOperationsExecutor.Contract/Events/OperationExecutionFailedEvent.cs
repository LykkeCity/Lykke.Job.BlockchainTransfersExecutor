using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Events
{
    /// <summary>
    /// Operation execution is failed
    /// </summary>
    [PublicAPI]
    [MessagePackObject(keyAsPropertyName: true)]
    public class OperationExecutionFailedEvent
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        public Guid OperationId { get; set; }

        /// <summary>
        /// Transaction ID
        /// </summary>
        public Guid TransactionId { get; set; }

        /// <summary>
        /// Error description
        /// </summary>
        public string Error { get; set; }

        /// <summary>
        /// Error code
        /// </summary>
        public OperationExecutionErrorCode ErrorCode { get; set; }
    }
}
