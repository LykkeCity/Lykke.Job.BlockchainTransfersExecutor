using System;
using ProtoBuf;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands
{
    [ProtoContract]
    public class ReleaseSourceAddressLockCommand
    {
        [ProtoMember(1)]
        public string BlockchainType { get; set; }

        [ProtoMember(2)]
        public string FromAddress { get; set; }

        [ProtoMember(3)]
        public Guid OperationId { get; set; }
    }
}
