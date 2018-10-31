using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Commands;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.OperationExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.OperationExecution;
using Lykke.Service.Assets.Client;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers.OperationExecution
{
    [UsedImplicitly]
    public class StartOneToManyOperationExecutionCommandsHandler
    {
        private readonly IAssetsServiceWithCache _assetsService;

        public StartOneToManyOperationExecutionCommandsHandler(IAssetsServiceWithCache assetsService)
        {
            _assetsService = assetsService;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(StartOneToManyOutputsExecutionCommand command, IEventPublisher publisher)
        {           
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

            publisher.PublishEvent
            (
                new OperationExecutionStartedEvent
                {
                    OperationId = command.OperationId,
                    FromAddress = command.FromAddress,
                    Outputs = command.Outputs,
                    BlockchainType = asset.BlockchainIntegrationLayerId,
                    BlockchainAssetId = asset.BlockchainIntegrationLayerAssetId,
                    AssetId = command.AssetId,
                    IncludeFee = command.IncludeFee,
                    EndpointsConfiguration = OperationExecutionEndpointsConfiguration.OneToMany
                }
            );

            return CommandHandlingResult.Ok();
        }
    }
}
