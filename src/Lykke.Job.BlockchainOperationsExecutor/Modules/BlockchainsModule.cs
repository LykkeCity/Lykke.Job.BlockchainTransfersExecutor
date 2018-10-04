using System.Linq;
using Autofac;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains;
using Lykke.Job.BlockchainOperationsExecutor.Services.Blockchains;
using Lykke.Job.BlockchainOperationsExecutor.Settings.Blockchain;
using Lykke.Job.BlockchainOperationsExecutor.Settings.JobSettings;
using Lykke.Job.BlockchainOperationsExecutor.Settings.SignFacade;
using Lykke.Service.BlockchainApi.Client;
using Lykke.Service.BlockchainSignFacade.Client;

namespace Lykke.Job.BlockchainOperationsExecutor.Modules
{
    public class BlockchainsModule : Module
    {
        private readonly BlockchainOperationsExecutorSettings _blockchainOperationsExecutorSettings;
        private readonly BlockchainsIntegrationSettings _blockchainsIntegrationSettings;
        private readonly BlockchainSignFacadeClientSettings _blockchainSignFacadeClientSettings;

        public BlockchainsModule(
            BlockchainOperationsExecutorSettings blockchainOperationsExecutorSettings,
            BlockchainsIntegrationSettings blockchainsIntegrationSettings,
            BlockchainSignFacadeClientSettings blockchainSignFacadeClientSettings)
        {
            _blockchainOperationsExecutorSettings = blockchainOperationsExecutorSettings;
            _blockchainsIntegrationSettings = blockchainsIntegrationSettings;
            _blockchainSignFacadeClientSettings = blockchainSignFacadeClientSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<BlockchainApiClientProvider>()
                .As<IBlockchainApiClientProvider>();

            builder.Register(ctx => CreateBlockchainSignFacadeClient(ctx.Resolve<ILogFactory>().CreateLog(this)))
                .As<IBlockchainSignFacadeClient>();

            foreach (var blockchain in _blockchainsIntegrationSettings.Blockchains.Where(b => !b.IsDisabled))
            {
                builder.Register(ctx =>
                    {
                        var logFactory = ctx.Resolve<ILogFactory>();
                        logFactory.CreateLog(this).Info(
                            "Blockchains registration",
                            "",
                            $"Registering blockchain: {blockchain.Type} -> \r\nAPI: {blockchain.ApiUrl}\r\nHW: {blockchain.HotWalletAddress}");
                        return new BlockchainApiClient(logFactory, blockchain.ApiUrl);
                    })
                    .Named<IBlockchainApiClient>(blockchain.Type);
            }
        }

        private IBlockchainSignFacadeClient CreateBlockchainSignFacadeClient(ILog log)
        {
            return new BlockchainSignFacadeClient
            (
                hostUrl: _blockchainSignFacadeClientSettings.ServiceUrl,
                apiKey: _blockchainOperationsExecutorSettings.SignFacadeApiKey,
                log: log
            );
        }
    }
}
