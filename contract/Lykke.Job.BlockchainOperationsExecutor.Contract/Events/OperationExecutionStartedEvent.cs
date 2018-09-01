using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Events
{
    /// <summary>
    /// Operation execution is started
    /// </summary>
    [PublicAPI]
    [MessagePackObject(keyAsPropertyName: true)]
    public class OperationExecutionStartedEvent
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        public Guid OperationId { get; set; }

        /// <summary>
        /// Source address in the blockchain
        /// </summary>
        public string FromAddress { get; set; }

        /// <summary>
        /// Destination
        /// </summary>
        public OperationOutput[] Outputs { get; set; }

        /// <summary>
        /// Lykke asset ID (not the blockchain one)
        /// </summary>
        public string AssetId { get; set; }

        /// <summary>
        /// Flag, which indicates, that the fee should be included
        /// in the specified amount
        /// </summary>
        public bool IncludeFee { get; set; }

        /// <summary>
        /// Blockchain type
        /// </summary>
        public string BlockchainType { get; set; }

        /// <summary>
        /// Blockchain asset ID
        /// </summary>
        public string BlockchainAssetId { get; set; }
    }
}
