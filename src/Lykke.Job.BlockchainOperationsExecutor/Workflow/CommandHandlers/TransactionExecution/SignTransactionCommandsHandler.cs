using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Lykke.Common.Chaos;
using Lykke.Cqrs;
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

        public SignTransactionCommandsHandler(
            IBlockchainSignFacadeClient signFacadeClient,
            IChaosKitty chaosKitty)
        {
            _signFacadeClient = signFacadeClient;
            _chaosKitty = chaosKitty;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(SignTransactionCommand command, IEventPublisher publisher)
        {
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

            publisher.PublishEvent(new TransactionSignedEvent
            {
                TransactionId = command.TransactionId,
                SignedTransaction = transactionSigningResult.SignedTransaction
            });

            return CommandHandlingResult.Ok();
        }
    }
}
