using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Core;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;
using Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events;
using Lykke.Service.Assets.Client;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers
{
    [UsedImplicitly]
    public class BuildTransactionCommandsHandler
    {
        private readonly RetryDelayProvider _retryDelayProvider;
        private readonly IBlockchainApiClientProvider _apiClientProvider;
        private readonly IAssetsServiceWithCache _assetsService;
        private readonly ISourceAddresLocksRepoistory _sourceAddresLocksRepoistory;

        public BuildTransactionCommandsHandler(
            RetryDelayProvider retryDelayProvider,
            IBlockchainApiClientProvider apiClientProvider,
            IAssetsServiceWithCache assetsService,
            ISourceAddresLocksRepoistory sourceAddresLocksRepoistory)
        {
            _retryDelayProvider = retryDelayProvider;
            _apiClientProvider = apiClientProvider;
            _assetsService = assetsService;
            _sourceAddresLocksRepoistory = sourceAddresLocksRepoistory;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(BuildTransactionCommand command, IEventPublisher publisher)
        {
            var isSourceAdressCaptured = await _sourceAddresLocksRepoistory.TryGetLockAsync(
                command.BlockchainType,
                command.FromAddress,
                command.OperationId);

            if (!isSourceAdressCaptured)
            {
                return CommandHandlingResult.Fail(_retryDelayProvider.SourceAddressLockingRetryDelay);
            }

            var apiClient = _apiClientProvider.Get(command.BlockchainType);
            var asset = await _assetsService.TryGetAssetAsync(command.AssetId);

            if (asset == null)
            {
                throw new InvalidOperationException("Asset not found");
            }

            // TODO: Cache it

            var blockchainAsset = await apiClient.GetAssetAsync(asset.BlockChainAssetId);

            var buildingResult = await apiClient.BuildTransactionAsync(
                command.OperationId,
                command.FromAddress,
                command.ToAddress,
                blockchainAsset,
                command.Amount,
                command.IncludeFee);

            ChaosKitty.Meow();

            publisher.PublishEvent(new TransactionBuiltEvent
            {
                OperationId = command.OperationId,
                BlockchainAssetId = blockchainAsset.AssetId,
                TransactionContext = buildingResult.TransactionContext
            });

            return CommandHandlingResult.Ok();
        }
    }
}
