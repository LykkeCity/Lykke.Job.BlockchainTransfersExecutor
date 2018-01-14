using System;
using ProtoBuf;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Events
{
    [ProtoContract]
    public class SourceAddressLockReleasedEvent
    {
        [ProtoMember(1)]
        public Guid OperationId { get; set; }
    }
}
