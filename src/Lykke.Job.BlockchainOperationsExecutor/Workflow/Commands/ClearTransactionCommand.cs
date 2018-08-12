using System;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands
{
    [MessagePackObject]
    public class ClearTransactionCommand
    {
        [Key(0)]
        public string BlockchainType { get; set; }

        [Key(1)]
        public Guid OperationId { get; set; }
    }
}
