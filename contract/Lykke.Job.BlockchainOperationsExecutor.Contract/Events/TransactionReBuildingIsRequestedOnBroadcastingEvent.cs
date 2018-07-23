﻿using System;
using JetBrains.Annotations;
using MessagePack;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Events
{    
    /// <summary>
    /// Blockchain transaction building repeats is requested on broadcasting
    /// </summary>
    [PublicAPI]
    [MessagePackObject]
    public class TransactionReBuildingIsRequestedOnBroadcastingEvent
    {        
        /// <summary>
        /// Lykke unique operation ID
        /// </summary>
        [Key(0)]
        public Guid OperationId { get; set; }
    }
}