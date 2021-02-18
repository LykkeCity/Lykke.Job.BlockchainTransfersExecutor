using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;
using Lykke.Job.BlockchainOperationsExecutor.Modules;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.TransactionExecution;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Job.BlockchainOperationsExecutor.Controllers
{
    [Route("api/maintenance")]
    public class MaintenanceController : ControllerBase
    {
        private readonly ICqrsEngine _cqrsEngine;
        private readonly ITransactionsToRebuildRepository _transactionsToRebuildRepository;

        public MaintenanceController(ICqrsEngine cqrsEngine, ITransactionsToRebuildRepository transactionsToRebuildRepository)
        {
            _cqrsEngine = cqrsEngine;
            _transactionsToRebuildRepository = transactionsToRebuildRepository;
        }

        [HttpGet("transactions/force-rebuild")]
        public async Task<ActionResult<IReadOnlyCollection<Guid>>> GetOperationsToRebuild([FromBody] WaitForTransactionEndingCommand command)
        {
            return Ok(await _transactionsToRebuildRepository.GetAll());
        }

        [HttpPost("transactions/force-rebuild/{operationId}")]
        public async Task AddOperationToRebuild(Guid operationId)
        {
            await _transactionsToRebuildRepository.AddOrReplace(operationId);
        }

        [HttpDelete("transactions/force-rebuild/{operationId}")]
        public async Task RemoveOperationToRebuild(Guid operationId)
        {
            await _transactionsToRebuildRepository.EnsureRemoved(operationId);
        }

        [HttpPost("commands/send-wait-for-transaction-ending")]
        public async Task SendWaitForTransactionEndingCommand([FromBody] WaitForTransactionEndingCommand command)
        {
            _cqrsEngine.SendCommand(command, $"{CqrsModule.TransactionExecutor}.saga", CqrsModule.TransactionExecutor);
        }

        [HttpPost("commands/send-sign-transaction-command")]
        public async Task SendSignTransactionCommand([FromBody] SignTransactionCommand command)
        {
            _cqrsEngine.SendCommand(command, $"{CqrsModule.TransactionExecutor}.saga", CqrsModule.TransactionExecutor);
        }

        [HttpPost("commands/send-broadcast-transaction-command")]
        public async Task SendBroadcastTransactionCommand([FromBody] BroadcastTransactionCommand command)
        {
            _cqrsEngine.SendCommand(command, $"{CqrsModule.TransactionExecutor}.saga", CqrsModule.TransactionExecutor);
        }
    }
}
