using System;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands
{
    [MessagePackObject]
    public class WaitForTransactionEndingCommand
    {
        [Key(0)]
        public string BlockchainType { get; set; }

        [Key(1)]
        public string BlockchainAssetId { get; set; }

        [Key(2)]
        public Guid OperationId { get; set; }

        [Key(3)]
        public DateTime OperationStartMoment { get; set; }

        [Key(4)]
        public bool WasBroadcasted { get; set; }
    }
}
