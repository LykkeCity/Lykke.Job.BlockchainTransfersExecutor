using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;
using Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands;
using Lykke.Service.Assets.Client;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers
{
    [UsedImplicitly]
    public class BuildTransactionCommandsHandler
    {
        private readonly IChaosKitty _chaosKitty;
        private readonly ILog _log;
        private readonly RetryDelayProvider _retryDelayProvider;
        private readonly IBlockchainApiClientProvider _apiClientProvider;
        private readonly IAssetsServiceWithCache _assetsService;
        private readonly ISourceAddresLocksRepoistory _sourceAddresLocksRepoistory;
        private readonly IBlockchainSignServiceClientProvider _signServiceClientProvider;

        public BuildTransactionCommandsHandler(
            IChaosKitty chaosKitty,
            ILog log,
            RetryDelayProvider retryDelayProvider,
            IBlockchainApiClientProvider apiClientProvider,
            IAssetsServiceWithCache assetsService,
            ISourceAddresLocksRepoistory sourceAddresLocksRepoistory,
            IBlockchainSignServiceClientProvider signServiceClientProvider)
        {
            _chaosKitty = chaosKitty;
            _log = log;
            _retryDelayProvider = retryDelayProvider;
            _apiClientProvider = apiClientProvider;
            _assetsService = assetsService;
            _sourceAddresLocksRepoistory = sourceAddresLocksRepoistory;
            _signServiceClientProvider = signServiceClientProvider;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(BuildTransactionCommand command, IEventPublisher publisher)
        {

            _log.WriteInfo(nameof(BuildTransactionCommand), command, "");

            var asset = await _assetsService.TryGetAssetAsync(command.AssetId);

            if (asset == null)
            {
                throw new InvalidOperationException("Asset not found");
            }

            if (string.IsNullOrWhiteSpace(asset.BlockchainIntegrationLayerId))
            {
                throw new InvalidOperationException("BlockchainIntegrationLayerId of the asset is not configured");
            }

            if (string.IsNullOrWhiteSpace(asset.BlockchainIntegrationLayerAssetId))
            {
                throw new InvalidOperationException("BlockchainIntegrationLayerAssetId of the asset is not configured");
            }

            var isSourceAdressCaptured = await _sourceAddresLocksRepoistory.TryGetLockAsync(
                asset.BlockchainIntegrationLayerId,
                command.FromAddress,
                command.OperationId);

            if (!isSourceAdressCaptured)
            {
                return CommandHandlingResult.Fail(_retryDelayProvider.SourceAddressLockingRetryDelay);
            }

            var apiClient = _apiClientProvider.Get(asset.BlockchainIntegrationLayerId);
            var signServiceClient = _signServiceClientProvider.Get(asset.BlockchainIntegrationLayerId);

            // TODO: Cache it

            var blockchainAsset = await apiClient.GetAssetAsync(asset.BlockchainIntegrationLayerAssetId);
            var wallet = await signServiceClient.GetWalletByPublicAddressAsync(command.FromAddress);

            var buildingResult = await apiClient.BuildSingleTransactionAsync(
                command.OperationId,
                command.FromAddress,
                wallet.AddressContext,
                command.ToAddress,
                blockchainAsset,
                command.Amount,
                command.IncludeFee);

            _chaosKitty.Meow(command.OperationId);

            publisher.PublishEvent(new TransactionBuiltEvent
            {
                OperationId = command.OperationId,
                BlockchainType = asset.BlockchainIntegrationLayerId,
                BlockchainAssetId = blockchainAsset.AssetId,
                TransactionContext = buildingResult.TransactionContext,
                FromAddressContext = wallet.AddressContext
            });

            return CommandHandlingResult.Ok();
        }
    }
}
