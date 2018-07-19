using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.BlockchainOperationsExecutor.Contract.Errors
{    
    /// <summary>
    /// Provides information about reason of buiding failure
    /// </summary>
    public enum TransactionBuildingErrorCode
    {
        Unknown,
        AmountTooSmall
    }
}
