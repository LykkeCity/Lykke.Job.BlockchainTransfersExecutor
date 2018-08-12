using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Events
{
    /// <summary>
    /// Blockchain transaction is built event
    /// </summary>
    [PublicAPI]
    [MessagePackObject]
    public class TransactionBuiltEvent
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        [Key(0)]
        public Guid OperationId { get; set; }

        /// <summary>
        /// Blockchain transaction context
        /// </summary>
        [Key(1)]
        public string TransactionContext { get; set; }

        /// <summary>
        /// Source address context
        /// </summary>
        [Key(2)]
        public string FromAddressContext { get; set; }
    }
}
