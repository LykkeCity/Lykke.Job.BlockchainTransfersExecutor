using System;
using JetBrains.Annotations;
using ProtoBuf;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Events
{
    /// <summary>
    /// Blockchain transaction is built event
    /// </summary>
    [PublicAPI]
    [ProtoContract]
    public class TransactionBuiltEvent
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        [ProtoMember(1)]
        public Guid OperationId { get; set; }

        /// <summary>
        /// Blockchain transaction context
        /// </summary>
        [ProtoMember(2)]
        public string TransactionContext { get; set; }

        /// <summary>
        /// Blockchain asset ID
        /// </summary>
        [ProtoMember(3)]
        public string BlockchainAssetId { get; set; }

        /// <summary>
        /// Blockchain type
        /// </summary>
        [ProtoMember(4)]
        public string BlockchainType { get; set; }
    }
}
