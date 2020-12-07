using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Modules;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.TransactionExecution;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Job.BlockchainOperationsExecutor.Controllers
{
    [Route("api/maintenance")]
    public class MaintenanceController : ControllerBase
    {
        private readonly ICqrsEngine _cqrsEngine;

        public MaintenanceController(ICqrsEngine cqrsEngine)
        {
            _cqrsEngine = cqrsEngine;
        }

        [HttpPost("commands/send-wait-for-transaction-ending")]
        public async void SendWaitForTransactionEndingCommand([FromBody] WaitForTransactionEndingCommand command)
        {
            _cqrsEngine.SendCommand(command, $"{CqrsModule.TransactionExecutor}.saga", CqrsModule.TransactionExecutor);
        }

        [HttpPost("commands/send-sign-transaction-command")]
        public async void SendWaitForTransactionEndingCommand([FromBody] SignTransactionCommand command)
        {
            _cqrsEngine.SendCommand(command, $"{CqrsModule.TransactionExecutor}.saga", CqrsModule.TransactionExecutor);
        }

        [HttpPost("commands/send-sign-transaction-command")]
        public async void SendWaitForTransactionEndingCommand([FromBody] BroadcastTransactionCommand command)
        {
            _cqrsEngine.SendCommand(command, $"{CqrsModule.TransactionExecutor}.saga", CqrsModule.TransactionExecutor);
        }
    }
}
