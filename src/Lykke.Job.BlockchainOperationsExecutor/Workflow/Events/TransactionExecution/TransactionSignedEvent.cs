using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution
{
    /// <summary>
    /// Blockchain transaction is signed event
    /// </summary>
    [MessagePackObject(keyAsPropertyName: true)]
    public class TransactionSignedEvent
    {
        [UsedImplicitly(ImplicitUseKindFlags.Access)]
        public Guid OperationId { get; set; }

        /// <summary>
        /// Lykke unique transaction ID
        /// </summary>
        public Guid TransactionId { get; set; }

        /// <summary>
        /// Signed transaction
        /// </summary>
        public string SignedTransaction { get; set; }
    }
}
