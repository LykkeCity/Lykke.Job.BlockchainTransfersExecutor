using System;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution
{
    public interface ITransactionExecutionEvent
    {
        Guid OperationId { get; set; }
        
        Guid TransactionId { get; set; }
    }
}
