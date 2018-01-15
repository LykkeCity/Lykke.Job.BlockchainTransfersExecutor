using System.Linq;
using Autofac;
using Common.Log;
using Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains;
using Lykke.Job.BlockchainOperationsExecutor.Services.Blockchains;
using Lykke.Job.BlockchainOperationsExecutor.Settings.Blockchain;
using Lykke.Service.BlockchainApi.Client;
using Lykke.Service.BlockchainSignService.Client;

namespace Lykke.Job.BlockchainOperationsExecutor.Modules
{
    public class BlockchainsModule : Module
    {
        private readonly BlockchainsIntegrationSettings _blockchainsIntegrationSettings;
        private readonly ILog _log;

        public BlockchainsModule(
            BlockchainsIntegrationSettings blockchainsIntegrationSettings,
            ILog log)
        {
            _blockchainsIntegrationSettings = blockchainsIntegrationSettings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BlockchainApiClientProvider>()
                .As<IBlockchainApiClientProvider>();

            builder.RegisterType<BlockchainSignServiceClientProvider>()
                .As<IBlockchainSignServiceClientProvider>();

            foreach (var blockchain in _blockchainsIntegrationSettings.Blockchains.Where(b => !b.IsDisabled))
            {
                _log.WriteInfo("Blockchains registration", "",
                    $"Registering blockchain: {blockchain.Type} -> \r\nAPI: {blockchain.ApiUrl}\r\nSign: {blockchain.SignFacadeUrl}\r\nHW: {blockchain.HotWalletAddress}");

                builder.RegisterType<BlockchainApiClient>()
                    .Named<IBlockchainApiClient>(blockchain.Type)
                    .WithParameter(TypedParameter.From(blockchain.ApiUrl));

                builder.RegisterType<BlockchainSignServiceClient>()
                    .Named<IBlockchainSignServiceClient>(blockchain.Type)
                    .WithParameter(TypedParameter.From(blockchain.SignFacadeUrl));
            }
        }
    }
}
