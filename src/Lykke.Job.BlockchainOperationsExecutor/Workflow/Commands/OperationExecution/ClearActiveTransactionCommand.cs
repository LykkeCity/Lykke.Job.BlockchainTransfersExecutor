using System;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.OperationExecution
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class ClearActiveTransactionCommand
    {
        public Guid OperationId { get; set; }
        public Guid TransactionId { get; set; }
        public int TransactionNumber { get; set; }
    }
}
