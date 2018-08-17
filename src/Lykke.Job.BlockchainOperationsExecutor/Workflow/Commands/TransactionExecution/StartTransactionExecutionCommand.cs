using System;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.TransactionExecution
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class StartTransactionExecutionCommand
    {
        public Guid OperationId { get; set; }
        public Guid TransactionId { get; set; }
        public string BlockchainType { get; set; }
        public string BlockchainAssetId { get; set; }
        public string AssetId { get; set; }
        public string FromAddress { get; set; }
        public string ToAddress { get; set; }
        public decimal Amount { get; set; }
        public bool IncludeFee { get; set; }
    }
}
