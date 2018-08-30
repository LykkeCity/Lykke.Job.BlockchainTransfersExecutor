using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Commands
{
    /// <summary>
    /// Command to start new blockchain operation execution
    /// </summary>
    [PublicAPI]
    [MessagePackObject(keyAsPropertyName: true)]
    public class StartOneToManyOutputsExecutionCommand
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
        /// Destination endpoints
        /// </summary>
        public OperationEndpoint[] ToEndpoints { get; set; }
        
        /// <summary>
        /// Lykke asset ID (not the blockchain one)
        /// </summary>
        public string AssetId { get; set; }    
        
        /// <summary>
        /// Flag, which indicates, that the fee should be included
        /// in the specified amount
        /// </summary>
        public bool IncludeFee { get; set; }
    }
}
