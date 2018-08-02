using System;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands
{
    [MessagePackObject]
    public class ForgetBroadcastedTransactionCommand
    {
        [Key(0)]
        public string BlockchainType { get; set; }

        [Key(1)]
        public Guid OperationId { get; set; }

        [Key(2)]
        public string FromAddress { get; set; }

        [Key(3)]
        public string ToAddress { get; set; }
    }
}
