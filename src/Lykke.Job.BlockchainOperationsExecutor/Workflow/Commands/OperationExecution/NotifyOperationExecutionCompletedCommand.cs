using System;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.OperationExecution
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class NotifyOperationExecutionCompletedCommand
    {
        public Guid OperationId { get; set; }
        public Guid TransactionId { get; set; }
        public string TransactionHash { get; set; }
        public decimal TransactionAmount { get; set; }
        public decimal TransactionFee { get; set; }        
        public long TransactionBlock { get; set; }
    }
}
