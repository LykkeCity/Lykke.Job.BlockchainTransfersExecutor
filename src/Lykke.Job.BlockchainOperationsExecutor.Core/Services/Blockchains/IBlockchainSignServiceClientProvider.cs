using Lykke.Service.BlockchainSignService.Client;

namespace Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains
{
    public interface IBlockchainSignServiceClientProvider
    {
        IBlockchainSignServiceClient Get(string blockchainType);
    }
}