using System;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Events
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class ActiveTransactionClearedEvent
    {
        public Guid OperationId { get; set; }
    }
}
