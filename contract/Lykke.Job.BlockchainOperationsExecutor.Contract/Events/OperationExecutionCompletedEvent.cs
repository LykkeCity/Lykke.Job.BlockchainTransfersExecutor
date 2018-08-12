using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Events
{
    /// <summary>
    /// Operation execution is completed
    /// </summary>
    [PublicAPI]
    [MessagePackObject]
    public class OperationExecutionCompletedEvent
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        [Key(0)]
        public Guid OperationId { get; set; }

        /// <summary>
        /// Transaction ID
        /// </summary>
        [Key(1)]
        public Guid TransactionId { get; set; }

        /// <summary>
        /// Hash of the blockchain transaction
        /// </summary>
        [Key(2)]
        public string TransactionHash { get; set; }

        /// <summary>
        /// Actual fee of the operation
        /// </summary>
        [Key(3)]
        public decimal Fee { get; set; }

        /// <summary>
        /// Actual underlying transaction amount.
        /// Single transaction can include multiple operations,
        /// so this value can include multiple operations amount
        /// </summary>
        [Key(4)]
        public decimal TransactionAmount { get; set; }

        /// <summary>
        /// Number of the block, transaction was included to
        /// </summary>
        [Key(5)]
        public long Block { get; set; }
    }
}
