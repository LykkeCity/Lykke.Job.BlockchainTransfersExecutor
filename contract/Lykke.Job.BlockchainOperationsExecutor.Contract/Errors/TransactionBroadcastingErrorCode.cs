using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Errors
{    
    /// <summary>
    /// Provides information about reason of broadcast failure
    /// </summary>
    public enum TransactionBroadcastingErrorCode
    {
        Unknown,
        AmountTooSmall
    }
}
