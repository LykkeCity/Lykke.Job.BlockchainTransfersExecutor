using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.TransactionExecution;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Events.TransactionExecution;
using Lykke.Service.BlockchainSignFacade.Client;
using Lykke.Service.BlockchainSignFacade.Contract.Models;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers.TransactionExecution
{
    [UsedImplicitly]
    public class SignTransactionCommandsHandler
    {
        private readonly IBlockchainSignFacadeClient _signFacadeClient;
        private readonly IChaosKitty _chaosKitty;
        private readonly ICommandHandlerEventRepository _commandHandlerEventRepository;

        private const string CommandHandlerId = "SignTransactionCommandsHandler";

        public SignTransactionCommandsHandler(
            IBlockchainSignFacadeClient signFacadeClient,
            IChaosKitty chaosKitty, 
            ICommandHandlerEventRepository commandHandlerEventRepository)
        {
            _signFacadeClient = signFacadeClient;
            _chaosKitty = chaosKitty;
            _commandHandlerEventRepository = commandHandlerEventRepository;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(SignTransactionCommand command, IEventPublisher publisher)
        {
            var alredyPublishedEvt = await _commandHandlerEventRepository.TryGetEventAsync(command.TransactionId, CommandHandlerId);

            if (alredyPublishedEvt != null)
            {
                publisher.PublishEvent(alredyPublishedEvt);

                return CommandHandlingResult.Ok();
            }

            var transactionSigningResult = await _signFacadeClient.SignTransactionAsync
            (
                blockchainType: command.BlockchainType,
                request: new SignTransactionRequest
                {
                    PublicAddresses = new[] { command.SignerAddress },
                    TransactionContext = command.TransactionContext
                }
            );

            _chaosKitty.Meow(command.TransactionId);

            if (string.IsNullOrWhiteSpace(transactionSigningResult?.SignedTransaction))
            {
                throw new InvalidOperationException("Sign service returned the empty transaction");
            }

            var evt = new TransactionSignedEvent
            {
                OperationId = command.OperationId,
                TransactionId = command.TransactionId,
                SignedTransaction = transactionSigningResult.SignedTransaction
            };

            await _commandHandlerEventRepository.InsertEventAsync(command.TransactionId, CommandHandlerId, evt);
            publisher.PublishEvent(evt);

            return CommandHandlingResult.Ok();
        }
    }
}
