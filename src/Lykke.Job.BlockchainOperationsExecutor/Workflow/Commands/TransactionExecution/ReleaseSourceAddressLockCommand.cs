using System;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.TransactionExecution
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class ReleaseSourceAddressLockCommand
    {
        public Guid TransactionId { get; set; }        
        public string BlockchainType { get; set; }
        public string FromAddress { get; set; }
        public bool AbortWorkflow { get; set; }
    }
}
