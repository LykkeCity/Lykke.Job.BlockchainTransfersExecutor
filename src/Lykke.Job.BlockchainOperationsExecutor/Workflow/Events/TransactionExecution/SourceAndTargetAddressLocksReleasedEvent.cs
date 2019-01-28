using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class SourceAndTargetAddressLocksReleasedEvent
    {
        [UsedImplicitly(ImplicitUseKindFlags.Access)]
        public Guid OperationId { get; set; }
        
        public Guid TransactionId { get; set; }
    }
}
