using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.BlockchainOperationsExecutor.Settings.Blockchain
{
    [UsedImplicitly]
    public class BlockchainSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string Type { get; set; }

        [HttpCheck("/api/isalive")]
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string ApiUrl { get; set; }

        [HttpCheck("/api/isalive")]
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string SignFacadeUrl { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string HotWalletAddress { get; set; }
    }
}
