using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.BlockchainOperationsExecutor.Settings.JobSettings
{
    [UsedImplicitly]
    public class BlockchainOperationsExecutorSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public DbSettings Db { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public CqrsSettings Cqrs { get; set; }

        [Optional]
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public ChaosSettings ChaosKitty { get; set; }
    }
}
