﻿using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.BlockchainOperationsExecutor.Settings.Blockchain
{
    [UsedImplicitly]
    public class BlockchainSettings
    {
        [Optional]
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public bool IsDisabled { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string Type { get; set; }

        [HttpCheck("/api/isalive")]
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string ApiUrl { get; set; }
        
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string HotWalletAddress { get; set; }
    }
}
