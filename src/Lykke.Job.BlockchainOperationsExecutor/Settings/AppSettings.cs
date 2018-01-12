using Lykke.Job.BlockchainOperationsExecutor.Settings.JobSettings;
using Lykke.Job.BlockchainOperationsExecutor.Settings.SlackNotifications;

namespace Lykke.Job.BlockchainOperationsExecutor.Settings
{
    public class AppSettings
    {
        public BlockchainOperationsExecutorSettings BlockchainOperationsExecutorJob { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}
