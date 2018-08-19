using System;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.TransactionExecution
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class SignTransactionCommand
    {
        public Guid OperationId { get; set; }
        public Guid TransactionId { get; set; }
        public string BlockchainType { get; set; }
        public string SignerAddress { get; set; }
        public string TransactionContext { get; set; }
    }
}
