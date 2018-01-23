using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Contract.Events;
using Lykke.Job.BlockchainOperationsExecutor.Core;
using Lykke.Job.BlockchainOperationsExecutor.Core.Services.Blockchains;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands;
using Lykke.Service.BlockchainSignService.Client.Models;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers
{
    [UsedImplicitly]
    public class SignTransactionCommandsHandler
    {
        private readonly ILog _log;
        private readonly IBlockchainSignServiceClientProvider _signServiceClientProvider;

        public SignTransactionCommandsHandler(
            ILog log, 
            IBlockchainSignServiceClientProvider signServiceClientProvider)
        {
            _log = log;
            _signServiceClientProvider = signServiceClientProvider;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(SignTransactionCommand command, IEventPublisher publisher)
        {
#if DEBUG
            _log.WriteInfo(nameof(SignTransactionCommand), command, "");
#endif
            var signServiceClient = _signServiceClientProvider.Get(command.BlockchainType);

            var transactionSigningResult = await signServiceClient.SignTransactionAsync(new SignRequestModel(
                new[] {command.SignerAddress},
                command.TransactionContext));

            ChaosKitty.Meow(command.OperationId);

            if (string.IsNullOrWhiteSpace(transactionSigningResult?.SignedTransaction))
            {
                throw new InvalidOperationException("Sign service return empty transaction");
            }

            publisher.PublishEvent(new TransactionSignedEvent
            {
                OperationId = command.OperationId,
                SignedTransaction = transactionSigningResult.SignedTransaction
            });

            return CommandHandlingResult.Ok();
        }
    }
}
