using JetBrains.Annotations;

namespace Lykke.Job.BlockchainOperationsExecutor.Settings.SlackNotifications
{
    [UsedImplicitly]
    public class SlackNotificationsSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public AzureQueuePublicationSettings AzureQueue { get; set; }
    }
}
