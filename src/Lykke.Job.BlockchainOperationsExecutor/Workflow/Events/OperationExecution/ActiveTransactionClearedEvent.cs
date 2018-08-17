using System;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.OperationExecution
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class ActiveTransactionClearedEvent
    {
        public Guid OperationId { get; set; }
    }
}
