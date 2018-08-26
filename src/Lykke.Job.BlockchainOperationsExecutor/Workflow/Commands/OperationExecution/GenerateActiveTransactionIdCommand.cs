using System;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.OperationExecution
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class GenerateActiveTransactionIdCommand
    {
        public Guid OperationId { get; set; }
        public int ActiveTransactioNumber { get; set; }
    }
}
