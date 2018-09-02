using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Events
{
    /// <summary>
    /// Execution of the operation with multiple outputs is completed
    /// </summary>
    [PublicAPI]
    [MessagePackObject(keyAsPropertyName: true)]
    public class OneToManyOperationExecutionCompletedEvent
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
        /// Actual fee of the transaction
        /// </summary>
        public decimal Fee { get; set; }

        /// <summary>
        /// Actual underlying transaction outputs.
        /// </summary>
        public OperationOutput[] TransactionOutputs { get; set; }

        /// <summary>
        /// Number of the block, transaction was included to
        /// </summary>
        public long Block { get; set; }
    }
}
