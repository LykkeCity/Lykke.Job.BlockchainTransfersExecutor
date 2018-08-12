using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Events
{
    /// <summary>
    /// Blockchain transaction is signed event
    /// </summary>
    [PublicAPI]
    [MessagePackObject]
    public class TransactionSignedEvent
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        [Key(0)]
        public Guid OperationId { get; set; }

        /// <summary>
        /// Signed transaction
        /// </summary>
        [Key(1)]
        public string SignedTransaction { get; set; }
    }
}
