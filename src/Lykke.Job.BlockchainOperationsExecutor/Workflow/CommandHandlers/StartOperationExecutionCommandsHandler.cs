using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Commands;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Service.Assets.Client;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers
{
    [UsedImplicitly]
    public class StartOperationExecutionCommandsHandler
    {
        private readonly IAssetsServiceWithCache _assetsService;

        public StartOperationExecutionCommandsHandler(IAssetsServiceWithCache assetsService)
        {
            _assetsService = assetsService;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(StartOperationExecutionCommand command, IEventPublisher publisher)
        {
            // TODO: In the further there could be a start of the operations aggregation.
            // Just by saving them to the storage for example.
            // Waiting period for aggregation should be passed in the command to let
            // the initiator of the command decide how long operation can wait to aggregate 
            // with other operations

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
            
            publisher.PublishEvent(new OperationExecutionStartedEvent
            {
                OperationId = command.OperationId,
                FromAddress = command.FromAddress,
                ToAddress = command.ToAddress,
                BlockchainType = asset.BlockchainIntegrationLayerId,
                BlockchainAssetId = asset.BlockchainIntegrationLayerAssetId,
                AssetId = command.AssetId,
                Amount = command.Amount,
                IncludeFee = command.IncludeFee
            });

            return CommandHandlingResult.Ok();
        }
    }
}
