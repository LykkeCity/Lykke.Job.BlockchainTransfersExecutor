using System;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class SourceAddressLockedEvent
    {
        public Guid TransactionId{ get; set; }
    }
}
