using System;
using ProtoBuf;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands
{
    [ProtoContract]
    public class SignTransactionCommand
    {
        [ProtoMember(1)]
        public string BlockchainType { get; set; }

        [ProtoMember(2)]
        public Guid OperationId { get; set; }

        [ProtoMember(3)]
        public string SignerAddress { get; set; }

        [ProtoMember(4)]
        public string TransactionContext { get; set; }
    }
}
