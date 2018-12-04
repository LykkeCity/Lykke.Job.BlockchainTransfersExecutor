using System;
using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.BlockchainOperationsExecutor.Settings.JobSettings
{
    [UsedImplicitly]
    public class CqrsSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        [AmqpCheck]
        public string RabbitConnectionString { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public TimeSpan RetryDelay { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public TimeSpan SourceAddressLockingRetryDelay { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public TimeSpan WaitForTransactionRetryDelay { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public TimeSpan NotEnoughBalanceRetryDelay { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public TimeSpan TransactionRebuildRetryDelay { get; set; }
    }
}
