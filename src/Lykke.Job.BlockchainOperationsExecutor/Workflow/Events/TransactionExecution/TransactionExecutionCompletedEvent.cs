using System;
using Lykke.Job.BlockchainOperationsExecutor.Contract;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class TransactionExecutionCompletedEvent
    {
        public Guid OperationId { get; set; }
        public Guid TransactionId { get; set; }
        public int TransactionNumber { get; set; }
        public long TransactionBlock { get; set; }
        public decimal TransactionFee { get; set; }
        public string TransactionHash { get; set; }
        public OperationOutput[] TransactionOutputs { get; set; }
    }
}
