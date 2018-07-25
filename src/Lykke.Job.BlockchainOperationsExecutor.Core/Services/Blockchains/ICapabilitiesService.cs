using System.Threading.Tasks;
using Lykke.Service.BlockchainApi.Client.Models;

namespace Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains
{
    public interface ICapabilitiesService
    {
        Task<BlockchainCapabilities> GetAsync(string blockchainType);
    }
}
