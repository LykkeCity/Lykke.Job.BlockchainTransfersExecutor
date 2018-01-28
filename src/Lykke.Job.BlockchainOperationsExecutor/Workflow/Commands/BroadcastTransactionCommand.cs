using System;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands
{
    [MessagePackObject]
    public class BroadcastTransactionCommand
    {
        [Key(0)]
        public string BlockchainType { get; set; }

        [Key(1)]
        public Guid OperationId { get; set; }

        [Key(2)]
        public string SignedTransaction { get; set; }
    }
}
