using System;
using JetBrains.Annotations;
using ProtoBuf;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Commands
{
    /// <summary>
    /// Command to start new blockchain operation
    /// </summary>
    [PublicAPI]
    [ProtoContract]
    public class StartOperationCommand
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        [ProtoMember(1)]
        public Guid OperationId { get; set; }

        /// <summary>
        /// Blockchain type
        /// </summary>
        [ProtoMember(2)]
        public string BlockchainType { get; set; }

        /// <summary>
        /// Source address in the blockchain
        /// </summary>
        [ProtoMember(3)]
        public string FromAddress { get; set; }

        /// <summary>
        /// Destination address in the blockchain
        /// </summary>
        [ProtoMember(4)]
        public string ToAddress { get; set; }

        /// <summary>
        /// Lykke asset ID (not the blockchain one)
        /// </summary>
        [ProtoMember(5)]
        public string AssetId { get; set; }

        /// <summary>
        /// Amount of funds to transfer
        /// </summary>
        [ProtoMember(6)]
        public decimal Amount { get; set; }

        /// <summary>
        /// Flag, which indicates, that the fee should be included
        /// in the specified amount
        /// </summary>
        [ProtoMember(7)]
        public bool IncludeFee { get; set; }
    }
}
