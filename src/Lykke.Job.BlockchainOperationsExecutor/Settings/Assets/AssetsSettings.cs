using System;
using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.BlockchainOperationsExecutor.Settings.Assets
{
    [UsedImplicitly]
    public class AssetsSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        [HttpCheck("/api/isalive")]
        public string ServiceUrl { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public TimeSpan CacheExpirationPeriod { get; set; }
    }
}
