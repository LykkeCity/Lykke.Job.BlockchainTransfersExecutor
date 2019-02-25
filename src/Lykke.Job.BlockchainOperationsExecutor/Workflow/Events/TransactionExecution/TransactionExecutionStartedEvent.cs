using System;
using Lykke.Job.BlockchainOperationsExecutor.Contract;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class TransactionExecutionStartedEvent : ITransactionExecutionEvent
    {
        public Guid OperationId { get; set; }
        public Guid TransactionId { get; set; }
        public int TransactionNumber { get; set; }
        public string FromAddress { get; set; }
        public OperationOutput[] Outputs { get; set; }
        public string BlockchainType { get; set; }
        public string BlockchainAssetId { get; set; }
        public string AssetId { get; set; }
        public bool IncludeFee { get; set; }
    }
}
