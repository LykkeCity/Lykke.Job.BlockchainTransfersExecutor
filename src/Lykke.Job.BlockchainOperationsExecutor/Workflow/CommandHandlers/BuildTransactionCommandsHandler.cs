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
using Lykke.Service.BlockchainApi.Client;
using Lykke.Service.BlockchainSignFacade.Client;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers
{
    [UsedImplicitly]
    public class BuildTransactionCommandsHandler
    {
        private readonly ILog _log;
        private readonly IChaosKitty _chaosKitty;
        private readonly RetryDelayProvider _retryDelayProvider;
        private readonly IBlockchainApiClientProvider _apiClientProvider;
        private readonly IAssetsServiceWithCache _assetsService;
        private readonly ISourceAddresLocksRepoistory _sourceAddresLocksRepoistory;
        private readonly IBlockchainSignFacadeClient _blockchainSignFacadeClient;
        private readonly ICapabilitiesService _capabilitiesService;

        public BuildTransactionCommandsHandler(
            ILog log,
            IChaosKitty chaosKitty,
            RetryDelayProvider retryDelayProvider,
            IBlockchainApiClientProvider apiClientProvider,
            IAssetsServiceWithCache assetsService,
            ISourceAddresLocksRepoistory sourceAddresLocksRepoistory,
            IBlockchainSignFacadeClient blockchainSignFacadeClient,
            ICapabilitiesService capabilitiesService)
        {
            _log = log.CreateComponentScope(nameof(BuildTransactionCommandsHandler));
            _chaosKitty = chaosKitty;
            _retryDelayProvider = retryDelayProvider;
            _apiClientProvider = apiClientProvider;
            _assetsService = assetsService;
            _sourceAddresLocksRepoistory = sourceAddresLocksRepoistory;
            _blockchainSignFacadeClient = blockchainSignFacadeClient;
            _capabilitiesService = capabilitiesService;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(BuildTransactionCommand command, IEventPublisher publisher)
        {
            // TODO: Should be remoed with next release
            // blockchainType and assetId should be obtained from the command

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

            var isFromAdressLocked = await _sourceAddresLocksRepoistory.TryGetLockAsync(
                asset.BlockchainIntegrationLayerId,
                command.FromAddress,
                command.OperationId);

            if (!isFromAdressLocked)
            {
                return CommandHandlingResult.Fail(_retryDelayProvider.SourceAddressLockingRetryDelay);
            }

            var capabilities = await _capabilitiesService.GetAsync(asset.BlockchainIntegrationLayerId);
            if (capabilities.IsExclusiveWithdrawalsRequired)
            {
                await _sourceAddresLocksRepoistory.TryGetLockAsync(
                    asset.BlockchainIntegrationLayerId,
                    command.ToAddress,
                    command.OperationId);
            }

            var apiClient = _apiClientProvider.Get(asset.BlockchainIntegrationLayerId);
            var blockchainAsset = await apiClient.GetAssetAsync(asset.BlockchainIntegrationLayerAssetId);
            var wallet = await _blockchainSignFacadeClient.GetWalletByPublicAddressAsync(asset.BlockchainIntegrationLayerId, command.FromAddress);

            try
            {
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
            catch (TransactionAlreadyBroadcastedException)
            {
                _log.WriteInfo
                (
                    nameof(BuildTransactionCommand),
                    command,
                    "API said, that transaction is already broadcasted"
                );

                publisher.PublishEvent(new TransactionBuildingRejectedEvent
                {
                    OperationId = command.OperationId
                });

                return CommandHandlingResult.Ok();
            }
        }
    }
}
