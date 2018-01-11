using Lykke.Job.BlockchainTransfersExecutor.Settings.JobSettings;
using Lykke.Job.BlockchainTransfersExecutor.Settings.SlackNotifications;

namespace Lykke.Job.BlockchainTransfersExecutor.Settings
{
    public class AppSettings
    {
        public BlockchainTransfersExecutorSettings BlockchainTransfersExecutorJob { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}
