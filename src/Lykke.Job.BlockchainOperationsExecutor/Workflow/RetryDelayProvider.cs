using System;
using JetBrains.Annotations;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow
{
    [UsedImplicitly(ImplicitUseKindFlags.InstantiatedNoFixedConstructorSignature)]
    public class RetryDelayProvider
    {
        public TimeSpan SourceAddressLockingRetryDelay { get; }
        public TimeSpan WaitForTransactionRetryDelay { get; }
        public TimeSpan NotEnoughBalanceRetryDelay { get; }

        public RetryDelayProvider(TimeSpan sourceAddressLockingRetryDelay, 
            TimeSpan waitForTransactionRetryDelay, 
            TimeSpan notEnoughBalanceRetryDelay)
        {
            SourceAddressLockingRetryDelay = sourceAddressLockingRetryDelay;
            WaitForTransactionRetryDelay = waitForTransactionRetryDelay;
            NotEnoughBalanceRetryDelay = notEnoughBalanceRetryDelay;
        }
    }
}
