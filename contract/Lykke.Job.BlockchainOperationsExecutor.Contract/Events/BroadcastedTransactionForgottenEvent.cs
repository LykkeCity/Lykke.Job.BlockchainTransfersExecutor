using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Events
{
    /// <summary>
    /// Transaction is removed from the list of the broadcasted transactions in the blockchain API 
    /// </summary>
    [PublicAPI]
    [MessagePackObject]
    public class BroadcastedTransactionForgottenEvent
    {
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        [Key(0)]
        public Guid OperationId { get; set; }
    }
}
