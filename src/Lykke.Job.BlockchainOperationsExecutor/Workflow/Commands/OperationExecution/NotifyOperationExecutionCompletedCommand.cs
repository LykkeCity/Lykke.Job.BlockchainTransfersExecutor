using System;
using Lykke.Job.BlockchainOperationsExecutor.Contract;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.OperationExecutions;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.OperationExecution
{
    [MessagePackObject(keyAsPropertyName: true)]
    public class NotifyOperationExecutionCompletedCommand
    {
        public Guid OperationId { get; set; }
        public Guid TransactionId { get; set; }
        public string TransactionHash { get; set; }
        public OperationOutput[] TransactionOutputs { get; set; }
        public decimal TransactionFee { get; set; }        
        public long TransactionBlock { get; set; }
        public OperationExecutionEndpointsConfiguration EndpointsConfiguration { get; set; }
    }
}
