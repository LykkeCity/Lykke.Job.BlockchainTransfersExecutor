using System;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.OperationExecution
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class ActiveTransactionIdGeneratedEvent
    {
        public Guid OperationId { get; set; }
        public Guid TransactionId { get; set; }
    }
}
