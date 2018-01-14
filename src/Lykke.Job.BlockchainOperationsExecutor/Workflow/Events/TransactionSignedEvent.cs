using System;
using ProtoBuf;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Events
{
    [ProtoContract]
    public class TransactionSignedEvent
    {
        [ProtoMember(1)]
        public Guid OperationId { get; set; }

        [ProtoMember(2)]
        public string SignedTransaction { get; set; }
    }
}
