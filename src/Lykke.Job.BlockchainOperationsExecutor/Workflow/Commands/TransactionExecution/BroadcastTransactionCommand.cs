using System;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.TransactionExecution
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class BroadcastTransactionCommand
    {
        public Guid OperationId { get; set; }
        public Guid TransactionId { get; set; }
        public string BlockchainType { get; set; }
        public string SignedTransaction { get; set; }
    }
}
