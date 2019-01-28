namespace Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains
{
    public interface IBlockchainSettingsProvider
    {
        string GetHotWalletAddress(string blockchainType);
        
        bool GetIsExclusiveWithdrawalsRequired(string blockchainType);
    }
}
