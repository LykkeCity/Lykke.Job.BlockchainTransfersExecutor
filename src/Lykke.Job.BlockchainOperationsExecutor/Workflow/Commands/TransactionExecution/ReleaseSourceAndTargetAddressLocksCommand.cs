using System;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.TransactionExecution
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class ReleaseSourceAndTargetAddressLocksCommand
    {
        public Guid OperationId { get; set; }
        
        public Guid TransactionId { get; set; }
        
        public string BlockchainType { get; set; }
        
        public string FromAddress { get; set; }
        
        public string ToAddress { get; set; }
        
        public bool AbortWorkflow { get; set; }
    }
}
