using System;
using ProtoBuf;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands
{
    [ProtoContract]
    public class ForgetBroadcastedTransactionCommand
    {
        [ProtoMember(1)]
        public string BlockchainType { get; set; }

        [ProtoMember(2)]
        public Guid OperationId { get; set; }
    }
}