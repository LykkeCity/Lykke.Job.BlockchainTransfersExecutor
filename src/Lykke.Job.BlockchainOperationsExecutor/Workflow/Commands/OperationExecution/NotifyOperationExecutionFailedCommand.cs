using System;
using Lykke.Job.BlockchainOperationsExecutor.Contract;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.OperationExecution
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class NotifyOperationExecutionFailedCommand
    {
        public Guid OperationId { get; set; }
        public Guid TransactionId { get; set; }
        public string Error { get; set; }
        public OperationExecutionErrorCode ErrorCode { get; set; }
    }
}
