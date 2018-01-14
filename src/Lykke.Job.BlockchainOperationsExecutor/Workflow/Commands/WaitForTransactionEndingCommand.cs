using System;
using ProtoBuf;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands
{
    [ProtoContract]
    public class WaitForTransactionEndingCommand
    {
        [ProtoMember(1)]
        public string BlockchainType { get; set; }

        [ProtoMember(2)]
        public string BlockchainAssetId { get; set; }

        [ProtoMember(3)]
        public Guid OperationId { get; set; }

        [ProtoMember(4)]
        public DateTime OperationStartMoment { get; set; }

        [ProtoMember(5)]
        public DateTime TransactionBroadcastingMoment { get; set; }
    }
}
