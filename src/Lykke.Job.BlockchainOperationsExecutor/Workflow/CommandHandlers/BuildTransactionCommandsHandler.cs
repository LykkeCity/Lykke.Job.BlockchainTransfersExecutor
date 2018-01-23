using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;
using Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands;
using Lykke.Service.Assets.Client;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers
{
    [UsedImplicitly]
    public class BuildTransactionCommandsHandler
    {
        private readonly ILog _log;
        private readonly RetryDelayProvider _retryDelayProvider;
        private readonly IBlockchainApiClientProvider _apiClientProvider;
        private readonly IAssetsServiceWithCache _assetsService;
        private readonly ISourceAddresLocksRepoistory _sourceAddresLocksRepoistory;

        public BuildTransactionCommandsHandler(
            ILog log,
            RetryDelayProvider retryDelayProvider,
            IBlockchainApiClientProvider apiClientProvider,
            IAssetsServiceWithCache assetsService,
            ISourceAddresLocksRepoistory sourceAddresLocksRepoistory)
        {
            _log = log;
            _retryDelayProvider = retryDelayProvider;
            _apiClientProvider = apiClientProvider;
            _assetsService = assetsService;
            _sourceAddresLocksRepoistory = sourceAddresLocksRepoistory;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(BuildTransactionCommand command, IEventPublisher publisher)
        {
#if DEBUG
            _log.WriteInfo(nameof(BuildTransactionCommand), command, "");
#endif
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
            
            // TODO: Cache it

            var blockchainAsset = await apiClient.GetAssetAsync(asset.BlockchainIntegrationLayerAssetId);

            var buildingResult = await apiClient.BuildTransactionAsync(
                command.OperationId,
                command.FromAddress,
                command.ToAddress,
                blockchainAsset,
                command.Amount,
                command.IncludeFee);

            ChaosKitty.Meow(command.OperationId);

            publisher.PublishEvent(new TransactionBuiltEvent
            {
                OperationId = command.OperationId,
                BlockchainType = asset.BlockchainIntegrationLayerId,
                BlockchainAssetId = blockchainAsset.AssetId,
                TransactionContext = buildingResult.TransactionContext
            });

            return CommandHandlingResult.Ok();
        }
    }
}
