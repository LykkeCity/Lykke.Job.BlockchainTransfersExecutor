using System;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class TransactionExecutionFailedEvent
    {
        public Guid OperationId { get; set; }
        public Guid TransactionId { get; set; }
        public TransactionExecutionResult ErrorCode { get; set; }
        public string Error { get; set; }
    }
}
