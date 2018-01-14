using JetBrains.Annotations;
using Lykke.Job.BlockchainOperationsExecutor.Settings.Assets;
using Lykke.Job.BlockchainOperationsExecutor.Settings.Blockchain;
using Lykke.Job.BlockchainOperationsExecutor.Settings.JobSettings;
using Lykke.Job.BlockchainOperationsExecutor.Settings.SlackNotifications;

namespace Lykke.Job.BlockchainOperationsExecutor.Settings
{
    [UsedImplicitly]
    public class AppSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public BlockchainOperationsExecutorSettings BlockchainOperationsExecutorJob { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public BlockchainsIntegrationSettings BlockchainsIntegration { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public SlackNotificationsSettings SlackNotifications { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public AssetsSettings Assets { get; set; }
    }
}
