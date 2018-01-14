using Lykke.Service.BlockchainApi.Client;

namespace Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains
{
    public interface IBlockchainApiClientProvider
    {
        IBlockchainApiClient Get(string blockchainType);
    }
}
