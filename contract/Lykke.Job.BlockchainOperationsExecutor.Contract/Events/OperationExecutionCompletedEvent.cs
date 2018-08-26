using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Events
{
    /// <summary>
    /// Operation execution is completed
    /// </summary>
    [PublicAPI]
    [MessagePackObject(keyAsPropertyName: true)]
    public class OperationExecutionCompletedEvent
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
        /// Hash of the blockchain transaction
        /// </summary>
        public string TransactionHash { get; set; }

        /// <summary>
        /// Actual fee of the operation
        /// </summary>
        public decimal Fee { get; set; }

        /// <summary>
        /// Actual underlying transaction amount.
        /// Single transaction can include multiple operations,
        /// so this value can include multiple operations amount
        /// </summary>
        public decimal TransactionAmount { get; set; }

        /// <summary>
        /// Number of the block, transaction was included to
        /// </summary>
        public long Block { get; set; }
    }
}
