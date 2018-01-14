using System;
using ProtoBuf;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Events
{
    [ProtoContract]
    public class TransactionBroadcastedEvent
    {
        [ProtoMember(1)]
        public Guid OperationId { get; set; }
    }

}
