using System;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.TransactionExecution
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class LockSourceAndTargetAddressesCommand
    {
        public Guid OperationId { get; set; }
        
        public Guid TransactionId { get; set; }
        
        public string FromAddress { get; set; }
        
        public string ToAddress { get; set; }
        
        public string BlockchainType { get; set; }
    }
}
