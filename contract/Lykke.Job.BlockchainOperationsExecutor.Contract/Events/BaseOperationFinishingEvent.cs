using System;
using JetBrains.Annotations;
using ProtoBuf;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Events
{
    /// <summary>
    /// Base class for the operation finishing events
    /// </summary>
    [PublicAPI]
    [ProtoContract]
    public abstract class BaseOperationFinishingEvent
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
        /// Transfer moment
        /// </summary>
        [ProtoMember(3)]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Source address in the blockchain
        /// </summary>
        [ProtoMember(4)]
        public string FromAddress { get; set; }

        /// <summary>
        /// Destination address in the blockchain
        /// </summary>
        [ProtoMember(5)]
        public string ToAddress { get; set; }

        /// <summary>
        /// Lykke asset ID (not the blockchain one)
        /// </summary>
        [ProtoMember(6)]
        public string AssetId { get; set; }

        /// <summary>
        /// Amount of funds to transfer
        /// </summary>
        [ProtoMember(7)]
        public decimal Amount { get; set; }


        /// <summary>
        /// Actual fee of the transfer
        /// </summary>
        [ProtoMember(8)]
        public decimal Fee { get; set; }
    }
}
