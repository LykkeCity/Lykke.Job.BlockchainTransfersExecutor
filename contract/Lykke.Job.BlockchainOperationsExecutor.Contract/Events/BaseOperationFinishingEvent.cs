using System;
using JetBrains.Annotations;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Events
{
    /// <summary>
    /// Base class for the operation finishing events
    /// </summary>
    [PublicAPI]
    public abstract class BaseOperationFinishingEvent
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        public Guid OperationId { get; set; }

        /// <summary>
        /// Blockchain type
        /// </summary>
        public string BlockchainType { get; set; }

        /// <summary>
        /// Transfer moment
        /// </summary>
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Source address in the blockchain
        /// </summary>
        public string FromAddress { get; set; }

        /// <summary>
        /// Destination address in the blockchain
        /// </summary>
        public string ToAddress { get; set; }

        /// <summary>
        /// Lykke asset ID (not the blockchain one)
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// Amount of funds to transfer
        /// </summary>
        public decimal Amount { get; set; }


        /// <summary>
        /// Actual fee of the transfer
        /// </summary>
        public decimal Fee { get; set; }
    }
}
