using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains;
using Lykke.Job.BlockchainOperationsExecutor.Mappers;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.TransactionExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution;
using Lykke.Service.BlockchainApi.Client.Models;
using Lykke.Service.BlockchainApi.Contract;
using Lykke.Service.BlockchainApi.Contract.Transactions;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers.TransactionExecution
{
    [UsedImplicitly]
    public class WaitForTransactionEndingCommandsHandler
    {
        private readonly ILog _log;
        private readonly RetryDelayProvider _delayProvider;
        private readonly IBlockchainApiClientProvider _apiClientProvider;

        public WaitForTransactionEndingCommandsHandler(
            ILog log,
            RetryDelayProvider delayProvider,
            IBlockchainApiClientProvider apiClientProvider)
        {
            _log = log;
            _delayProvider = delayProvider;
            _apiClientProvider = apiClientProvider;}

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(WaitForTransactionEndingCommand command, IEventPublisher publisher)
        {
            var apiClient = _apiClientProvider.Get(command.BlockchainType);

            // TODO: Cache it

            var blockchainAsset = await apiClient.GetAssetAsync(command.BlockchainAssetId);
            BaseBroadcastedTransaction transaction;
            OperationOutput[] transactionOutputs = null;

            if (command.Outputs.Length > 1)
            {
                var manyOutputsTransaction = await apiClient.TryGetBroadcastedTransactionWithManyOutputsAsync
                (
                    command.TransactionId,
                    blockchainAsset
                );

                transaction = manyOutputsTransaction;

                if (manyOutputsTransaction != null)
                {
                    transactionOutputs = manyOutputsTransaction.Outputs
                        .Select(o => new OperationOutput
                        {
                            Address = o.ToAddress,
                            Amount = o.Amount
                        })
                        .ToArray();
                }
            }
            else if(command.Outputs.Length == 1)
            {
                var singleTransaction = await apiClient.TryGetBroadcastedSingleTransactionAsync
                (
                    command.TransactionId, 
                    blockchainAsset
                );

                transaction = singleTransaction;

                if (singleTransaction != null)
                {
                    transactionOutputs = new[]
                    {
                        new OperationOutput
                        {
                            Address = command.Outputs.Single().Address,
                            Amount = singleTransaction.Amount
                        }
                    };
                }
            }
            else
            {
                throw new InvalidOperationException("There should be at least one output");
            }

            if (transaction == null)
            {
                _log.WriteInfo
                (
                    nameof(WaitForTransactionEndingCommand),
                    command,
                    "Blockchain API returned no transaction. Assuming, that it's already was cleared"
                );

                // Transaction already has been forgotten, this means, 
                // that process has been went further and no events should be generated here.

                return CommandHandlingResult.Ok();
            }

            if (transactionOutputs == null)
            {
                throw new InvalidOperationException("Transaction outputs should be not null here");
            }
            
            switch (transaction.State)
            {
                case BroadcastedTransactionState.InProgress:
                    
                    return CommandHandlingResult.Fail(_delayProvider.WaitForTransactionRetryDelay);

                case BroadcastedTransactionState.Completed:

                    publisher.PublishEvent(new TransactionExecutionCompletedEvent
                    {
                        OperationId = command.OperationId,
                        TransactionId = command.TransactionId,
                        TransactionNumber = command.TransactionNumber,
                        TransactionHash = transaction.Hash,
                        TransactionOutputs = transactionOutputs,
                        TransactionFee = transaction.Fee,
                        TransactionBlock = transaction.Block
                    });

                    return CommandHandlingResult.Ok();

                case BroadcastedTransactionState.Failed:

                    if (transaction.ErrorCode == BlockchainErrorCode.NotEnoughBalance ||
                        transaction.ErrorCode == BlockchainErrorCode.BuildingShouldBeRepeated)
                    {
                        publisher.PublishEvent(new TransactionExecutionRepeatRequestedEvent
                        {
                            OperationId = command.OperationId,
                            TransactionId = command.TransactionId,
                            TransactionNumber = command.TransactionNumber,
                            ErrorCode = transaction.ErrorCode.Value.MapToTransactionExecutionResult(),
                            Error = transaction.Error
                        });
                    }
                    else
                    {
                        publisher.PublishEvent(new TransactionExecutionFailedEvent
                        {
                            OperationId = command.OperationId,
                            TransactionId = command.TransactionId,
                            TransactionNumber = command.TransactionNumber,
                            ErrorCode = transaction.ErrorCode?.MapToTransactionExecutionResult() ?? TransactionExecutionResult.UnknownError,
                            Error = transaction.Error
                        });
                    }

                    return CommandHandlingResult.Ok();

                default:
                    throw new ArgumentOutOfRangeException
                    (
                        nameof(transaction.State),
                        $"Transaction state [{transaction.State}] is not supported."
                    );
            }
        }
    }
}
