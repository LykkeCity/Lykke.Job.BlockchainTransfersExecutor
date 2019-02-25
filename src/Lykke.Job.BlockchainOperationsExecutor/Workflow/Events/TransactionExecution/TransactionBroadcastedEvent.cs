using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution
{
    /// <summary>
    /// Blockchain transaction is broadcasted event
    /// </summary>
    [MessagePackObject(keyAsPropertyName: true)]
    public class TransactionBroadcastedEvent : ITransactionExecutionEvent
    {
        [UsedImplicitly(ImplicitUseKindFlags.Access)]
        public Guid OperationId { get; set; }
        /// <summary>
        /// Transaction ID, which is broadcasted
        /// </summary>
        public Guid TransactionId { get; set; }
    }

}
