using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Events
{
    /// <summary>
    /// Operation execution is started
    /// </summary>
    [PublicAPI]
    [MessagePackObject]
    public class OperationExecutionStartedEvent
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        [Key(0)]
        public Guid OperationId { get; set; }

        /// <summary>
        /// Lykke unique transaction ID
        /// </summary>
        [Key(1)]
        public Guid TransactionId { get; set; }

        /// <summary>
        /// Source address in the blockchain
        /// </summary>
        [Key(2)]
        public string FromAddress { get; set; }

        /// <summary>
        /// Destination address in the blockchain
        /// </summary>
        [Key(3)]
        public string ToAddress { get; set; }

        /// <summary>
        /// Lykke asset ID (not the blockchain one)
        /// </summary>
        [Key(4)]
        public string AssetId { get; set; }

        /// <summary>
        /// Amount of funds to transfer
        /// </summary>
        [Key(5)]
        public decimal Amount { get; set; }

        /// <summary>
        /// Flag, which indicates, that the fee should be included
        /// in the specified amount
        /// </summary>
        [Key(6)]
        public bool IncludeFee { get; set; }

        /// <summary>
        /// Blockchain type
        /// </summary>
        [Key(7)]
        public string BlockchainType { get; set; }

        /// <summary>
        /// Blockchain asset ID
        /// </summary>
        [Key(8)]
        public string BlockchainAssetId { get; set; }
    }
}
