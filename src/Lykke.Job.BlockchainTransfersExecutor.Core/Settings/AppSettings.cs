using Lykke.Job.BlockchainTransfersExecutor.Core.Settings.JobSettings;
using Lykke.Job.BlockchainTransfersExecutor.Core.Settings.SlackNotifications;

namespace Lykke.Job.BlockchainTransfersExecutor.Core.Settings
{
    public class AppSettings
    {
        public BlockchainTransfersExecutorSettings BlockchainTransfersExecutorJob { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}