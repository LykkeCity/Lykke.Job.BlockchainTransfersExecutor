using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains;
using Lykke.Job.BlockchainOperationsExecutor.Mappers;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.TransactionExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution;
using Lykke.Service.Assets.Client;
using Lykke.Service.BlockchainApi.Client;
using Lykke.Service.BlockchainApi.Client.Models;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainSignFacade.Client;
using ErrorResponseException = Lykke.Service.BlockchainApi.Client.ErrorResponseException;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers.TransactionExecution
{
    [UsedImplicitly]
    public class BuildTransactionCommandsHandler
    {
        private readonly ILog _log;
        private readonly IChaosKitty _chaosKitty;
        private readonly RetryDelayProvider _retryDelayProvider;
        private readonly IBlockchainApiClientProvider _apiClientProvider;
        private readonly IBlockchainSignFacadeClient _blockchainSignFacadeClient;

        public BuildTransactionCommandsHandler(
            ILogFactory logFactory,
            IChaosKitty chaosKitty,
            RetryDelayProvider retryDelayProvider,
            IBlockchainApiClientProvider apiClientProvider,
            IAssetsServiceWithCache assetsService,
            ISourceAddresLocksRepoistory sourceAddresLocksRepoistory,
            IBlockchainSignFacadeClient blockchainSignFacadeClient)
        {
            _log = logFactory.CreateLog(this);
            _chaosKitty = chaosKitty;
            _retryDelayProvider = retryDelayProvider;
            _apiClientProvider = apiClientProvider;
            _blockchainSignFacadeClient = blockchainSignFacadeClient;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(BuildTransactionCommand command, IEventPublisher publisher)
        {
            var apiClient = _apiClientProvider.Get(command.BlockchainType);
            var blockchainAsset = await apiClient.GetAssetAsync(command.BlockchainAssetId);
            var wallet = await _blockchainSignFacadeClient.GetWalletByPublicAddressAsync(command.BlockchainType, command.FromAddress);

            try
            {
                TransactionBuildingResult buildingResult;

                if (command.ToEndpoints.Length > 1)
                {
                    buildingResult = await apiClient.BuildTransactionWithManyOutputsAsync
                    (
                        command.TransactionId,
                        command.FromAddress,
                        wallet.AddressContext,
                        command.ToEndpoints.Select(p => new BuildingTransactionOutput(p.Address, p.Amount)),
                        blockchainAsset
                    );
                }
                else if(command.ToEndpoints.Length == 1)
                {
                    var destination = command.ToEndpoints.Single();

                    buildingResult = await apiClient.BuildSingleTransactionAsync
                    (
                        command.TransactionId,
                        command.FromAddress,
                        wallet.AddressContext,
                        destination.Address,
                        blockchainAsset,
                        destination.Amount,
                        command.IncludeFee
                    );
                }
                else
                {
                    throw new InvalidOperationException("There should be at least one destination endpoint");
                }

                _chaosKitty.Meow(command.TransactionId);

                publisher.PublishEvent(new TransactionBuiltEvent
                {
                    OperationId = command.OperationId,
                    TransactionId = command.TransactionId,
                    TransactionContext = buildingResult.TransactionContext,
                    FromAddressContext = wallet.AddressContext
                });

                return CommandHandlingResult.Ok();
            }
            catch (ErrorResponseException e) when(e.ErrorCode == BlockchainErrorCode.AmountIsTooSmall)
            {
                _log.Warning("API said, that amount is too small", context: command);

                publisher.PublishEvent(new TransactionExecutionFailedEvent
                {
                    OperationId = command.OperationId,
                    TransactionId = command.TransactionId,
                    TransactionNumber = command.TransactionNumber,
                    ErrorCode = e.ErrorCode.MapToTransactionExecutionResult(),
                    Error = e.Error.GetSummaryMessage()
                });

                return CommandHandlingResult.Ok();
            }
            catch (ErrorResponseException e) when (e.ErrorCode == BlockchainErrorCode.NotEnoughBalance)
            {
                _log.Info("API said, that balance is not enough to proceed the transaction", command);
                return CommandHandlingResult.Fail(_retryDelayProvider.NotEnoughBalanceRetryDelay);
            }
            catch (TransactionAlreadyBroadcastedException)
            {
                _log.Info("API said, that transaction already was broadcasted", command);

                publisher.PublishEvent(new TransactionBuildingRejectedEvent
                {
                    OperationId = command.OperationId,
                    TransactionId = command.TransactionId
                });

                return CommandHandlingResult.Ok();
            }
        }
    }
}
