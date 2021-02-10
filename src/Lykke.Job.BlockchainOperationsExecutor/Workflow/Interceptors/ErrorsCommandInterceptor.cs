using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Abstractions.Middleware;
using Lykke.Cqrs.Middleware;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;

namespace Lykke.Job.BlockchainOperationsExecutor.Workflow.Interceptors
{
    public class ErrorsCommandInterceptor : ICommandInterceptor
    {
        private readonly ILog _log;

        public ErrorsCommandInterceptor(ILogFactory logFactory)
        {
            _log = logFactory.CreateLog(this);
        }

        public async Task<CommandHandlingResult> InterceptAsync(ICommandInterceptionContext context)
        {
            try
            {
                return await context.InvokeNextAsync();
            }
            catch (UnexpectedEventException ex)
            {
                _log.Warning($"{nameof(UnexpectedEventException)} handled", ex);
                return CommandHandlingResult.Fail(TimeSpan.FromSeconds(10));
            }
        }
    }
}
