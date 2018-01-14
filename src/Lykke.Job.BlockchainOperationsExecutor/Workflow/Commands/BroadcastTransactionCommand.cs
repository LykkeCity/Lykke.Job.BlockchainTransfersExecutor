using System;
using ProtoBuf;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands
{
    [ProtoContract]
    public class BroadcastTransactionCommand
    {
        [ProtoMember(1)]
        public string BlockchainType { get; set; }

        [ProtoMember(2)]
        public Guid OperationId { get; set; }

        [ProtoMember(3)]
        public string SignedTransaction { get; set; }
    }
}
