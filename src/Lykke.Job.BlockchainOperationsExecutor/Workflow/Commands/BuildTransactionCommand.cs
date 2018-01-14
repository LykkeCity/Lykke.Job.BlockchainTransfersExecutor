using System;
using ProtoBuf;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands
{
    [ProtoContract]
    public class BuildTransactionCommand
    {
        [ProtoMember(1)]
        public string BlockchainType { get; set; }

        [ProtoMember(2)]
        public string AssetId { get; set; }

        [ProtoMember(3)]
        public Guid OperationId { get; set; }

        [ProtoMember(4)]
        public string FromAddress { get; set; }

        [ProtoMember(5)]
        public string ToAddress { get; set; }

        [ProtoMember(6)]
        public decimal Amount { get; set; }

        [ProtoMember(7)]
        public bool IncludeFee { get; set; }
    }
}
