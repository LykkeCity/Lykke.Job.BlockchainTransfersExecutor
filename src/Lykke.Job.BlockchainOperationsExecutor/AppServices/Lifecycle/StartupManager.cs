using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Cqrs;

namespace Lykke.Job.BlockchainOperationsExecutor.AppServices.Lifecycle
{
    // NOTE: Sometimes, startup process which is expressed explicitly is not just better, 
    // but the only way. If this is your case, use this class to manage startup.
    // For example, sometimes some state should be restored before any periodical handler will be started, 
    // or any incoming message will be processed and so on.
    // Do not forget to remove As<IStartable>() and AutoActivate() from DI registartions of services, 
    // which you want to startup explicitly.
    [UsedImplicitly]
    public class StartupManager : IStartupManager
    {
        private readonly ILog _log;
        private readonly ICqrsEngine _cqrsEngine;

        public StartupManager(
            ILog log, 
            ICqrsEngine cqrsEngine)
        {
            _log = log;
            _cqrsEngine = cqrsEngine;
        }

        public async Task StartAsync()
        {
            _log.WriteInfo(nameof(StartAsync), null, "Starting Cqrs engine...");

            _cqrsEngine.Start();

            await Task.CompletedTask;
        }
    }
}
