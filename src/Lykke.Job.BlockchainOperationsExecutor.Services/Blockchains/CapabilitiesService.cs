using Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains;
using Lykke.Service.BlockchainApi.Client.Models;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace Lykke.Job.BlockchainOperationsExecutor.Services.Blockchains
{
    public class CapabilitiesService : ICapabilitiesService
    {
        private readonly IBlockchainApiClientProvider _blockchainApiClientProvider;
        private readonly ConcurrentDictionary<string, BlockchainCapabilities> _cache;
        private readonly ConcurrentDictionary<string, SemaphoreSlim> _locks;

        public CapabilitiesService(IBlockchainApiClientProvider blockchainApiClientProvider)
        {
            _blockchainApiClientProvider = blockchainApiClientProvider;
            _cache = new ConcurrentDictionary<string, BlockchainCapabilities>();
            _locks = new ConcurrentDictionary<string, SemaphoreSlim>();
        }

        public async Task<BlockchainCapabilities> GetAsync(string blockchainType)
        {
            if (_cache.TryGetValue(blockchainType, out var value))
            {
                return value;
            }

            var apiClient = _blockchainApiClientProvider.Get(blockchainType);
            var @lock = _locks.GetOrAdd(blockchainType, x => new SemaphoreSlim(1));

            await @lock.WaitAsync();

            try
            {
                var capabilities = await apiClient.GetCapabilitiesAsync();

                _cache.TryAdd(blockchainType, capabilities);

                return capabilities;
            }
            finally
            {
                @lock.Release();
            }
        }
    }
}
