using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Commands
{
    /// <summary>
    /// Command to start new blockchain operation execution
    /// </summary>
    [PublicAPI]
    [MessagePackObject]
    public class StartOperationExecutionCommand
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        [Key(0)]
        public Guid OperationId { get; set; }

        /// <summary>
        /// Source address in the blockchain
        /// </summary>
        [Key(1)]
        public string FromAddress { get; set; }

        /// <summary>
        /// Destination address in the blockchain
        /// </summary>
        [Key(2)]
        public string ToAddress { get; set; }

        /// <summary>
        /// Lykke asset ID (not the blockchain one)
        /// </summary>
        [Key(3)]
        public string AssetId { get; set; }

        /// <summary>
        /// Amount of funds to transfer
        /// </summary>
        [Key(4)]
        public decimal Amount { get; set; }

        /// <summary>
        /// Flag, which indicates, that the fee should be included
        /// in the specified amount
        /// </summary>
        [Key(5)]
        public bool IncludeFee { get; set; }
    }
}
