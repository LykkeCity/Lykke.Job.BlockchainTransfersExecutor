using System;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.TransactionExecution
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class LockSourceAddressCommand
    {
        public Guid OperationId { get; set; }
        public Guid TransactionId { get; set; }
        public string FromAddress { get; set; }
        public string BlockchainType { get; set; }
    }
}
