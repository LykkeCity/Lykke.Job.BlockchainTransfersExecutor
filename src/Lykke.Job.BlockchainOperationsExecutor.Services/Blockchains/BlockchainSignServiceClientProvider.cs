using System;
using Autofac.Features.Indexed;
using JetBrains.Annotations;
using Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains;
using Lykke.Service.BlockchainSignService.Client;

namespace Lykke.Job.BlockchainOperationsExecutor.Services.Blockchains
{
    [UsedImplicitly]
    public class BlockchainSignServiceClientProvider : IBlockchainSignServiceClientProvider
    {
        private readonly IIndex<string, IBlockchainSignServiceClient> _clients;

        public BlockchainSignServiceClientProvider(IIndex<string, IBlockchainSignServiceClient> clients)
        {
            _clients = clients;
        }

        public IBlockchainSignServiceClient Get(string blockchainType)
        {
            if (!_clients.TryGetValue(blockchainType, out var client))
            {
                throw new InvalidOperationException($"Blockchain sign facade client [{blockchainType}] is not found");
            }

            return client;
        }
    }
}
