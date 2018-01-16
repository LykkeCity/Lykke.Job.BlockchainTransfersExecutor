using System;
using ProtoBuf;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Events
{
    [ProtoContract]
    public class TransactionBuiltEvent
    {
        [ProtoMember(1)]
        public Guid OperationId { get; set; }

        [ProtoMember(2)]
        public string TransactionContext { get; set; }

        [ProtoMember(3)]
        public string BlockchainAssetId { get; set; }

        [ProtoMember(4)]
        public string BlockchainType { get; set; }
    }
}
