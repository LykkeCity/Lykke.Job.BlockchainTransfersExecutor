using System;
using System.Collections.Concurrent;
using Castle.Core.Logging;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Job.BlockchainOperationsExecutor.Modules;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.Commands.TransactionExecution;
using Microsoft.AspNetCore.Mvc;

namespace Lykke.Job.BlockchainOperationsExecutor.Controllers
{
    public static class AllowedWithdrawals
    {
        public static readonly ConcurrentDictionary<Guid,bool> List = new ConcurrentDictionary<Guid, bool>();
    }

    [Route("api/maintenance")]
    public class MaintenanceController : ControllerBase
    {
        private readonly ICqrsEngine _cqrsEngine;
        private ILog _log;

        public MaintenanceController(ICqrsEngine cqrsEngine, ILogFactory logFactory)
        {
            _log = logFactory.CreateLog(this);
            _cqrsEngine = cqrsEngine;
        }

        [HttpPost("commands/send-wait-for-transaction-ending")]
        public async void SendWaitForTransactionEndingCommand([FromBody] WaitForTransactionEndingCommand command)
        {
            _cqrsEngine.SendCommand(command, $"{CqrsModule.TransactionExecutor}.saga", CqrsModule.TransactionExecutor);
        }

        [HttpPost("commands/allow-withdrawal")]
        public async void SendWaitForTransactionEndingCommand([FromBody] AllowWithdrawalRequest request)
        {
            _log.Warning("Operation added to the allowed list", context: request);

            AllowedWithdrawals.List.TryAdd(request.OperationId, true);
        }

        public class AllowWithdrawalRequest
        {
            public Guid OperationId { get; set; }
        }
    }
}
