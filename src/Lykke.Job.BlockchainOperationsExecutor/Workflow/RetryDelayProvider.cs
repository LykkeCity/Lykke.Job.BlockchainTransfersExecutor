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
        public TimeSpan RebuildingConfirmationCheckRetryDelay { get; }

        public RetryDelayProvider(TimeSpan sourceAddressLockingRetryDelay, 
            TimeSpan waitForTransactionRetryDelay, 
            TimeSpan notEnoughBalanceRetryDelay,
            TimeSpan rebuildingConfirmationCheckRetryDelay)
        {
            SourceAddressLockingRetryDelay = sourceAddressLockingRetryDelay;
            WaitForTransactionRetryDelay = waitForTransactionRetryDelay;
            NotEnoughBalanceRetryDelay = notEnoughBalanceRetryDelay;
            RebuildingConfirmationCheckRetryDelay = rebuildingConfirmationCheckRetryDelay;
        }
    }
}
