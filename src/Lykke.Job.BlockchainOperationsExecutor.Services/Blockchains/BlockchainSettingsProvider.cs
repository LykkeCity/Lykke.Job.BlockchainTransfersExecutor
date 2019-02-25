using System;
using System.Collections.Generic;
using Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains;

namespace Lykke.Job.BlockchainOperationsExecutor.Services.Blockchains
{
    public class BlockchainSettingsProvider : IBlockchainSettingsProvider
    {
        private readonly IReadOnlyDictionary<string, string> _hotWalletAddressSettings;
        private readonly IReadOnlyDictionary<string, bool> _exclusiveWithdrawalsRequiredSettings;

        public BlockchainSettingsProvider(
            IReadOnlyDictionary<string, string> hotWalletAddressSettings,
            IReadOnlyDictionary<string, bool> exclusiveWithdrawalsRequiredSettings)
        {
            _hotWalletAddressSettings = hotWalletAddressSettings;
            _exclusiveWithdrawalsRequiredSettings = exclusiveWithdrawalsRequiredSettings;
        }
        
        public string GetHotWalletAddress(string blockchainType)
        {
            return GetSettingValue(_hotWalletAddressSettings, blockchainType);
        }

        public bool GetExclusiveWithdrawalsRequired(string blockchainType)
        {
            return GetSettingValue(_exclusiveWithdrawalsRequiredSettings, blockchainType);
        }

        private static T GetSettingValue<T>(IReadOnlyDictionary<string, T> settings, string blockchainType)
        {
            if(!settings.TryGetValue(blockchainType, out var value))
            {
                throw new InvalidOperationException($"Blockchain [{blockchainType}] settings are not found");
            }

            return value;
        }
    }
}
