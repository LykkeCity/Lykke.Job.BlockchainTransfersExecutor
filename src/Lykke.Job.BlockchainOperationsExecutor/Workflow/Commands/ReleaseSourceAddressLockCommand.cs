using System;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands
{
    [MessagePackObject]
    public class ReleaseSourceAddressLockCommand
    {
        [Key(0)]
        public string BlockchainType { get; set; }

        [Key(1)]
        public string FromAddress { get; set; }

        [Key(2)]
        public Guid OperationId { get; set; }
        
        [Key(3)]
        public bool BuildingRepeatsIsRequested { get; set; }

        [Key(4)]
        public bool WasBroadcasted { get; set; }
    }
}
