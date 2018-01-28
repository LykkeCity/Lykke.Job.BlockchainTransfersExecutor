using Autofac;
using Common.Log;
using Lykke.Job.BlockchainOperationsExecutor.AzureRepositories;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain;
using Lykke.Job.BlockchainOperationsExecutor.Settings.JobSettings;
using Lykke.SettingsReader;

namespace Lykke.Job.BlockchainOperationsExecutor.Modules
{
    public class RepositoriesModule : Module
    {
        private readonly IReloadingManager<DbSettings> _dbSettings;
        private readonly ILog _log;

        public RepositoriesModule(
            IReloadingManager<DbSettings> dbSettings,
            ILog log)
        {
            _log = log;
            _dbSettings = dbSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => OperationExecutionsRepository.Create(_dbSettings.Nested(x => x.DataConnString), _log))
                .As<IOperationExecutionsRepository>();

            builder.Register(c => SourceAddressLocksRepository.Create(_dbSettings.Nested(x => x.DataConnString), _log))
                .As<ISourceAddresLocksRepoistory>();
        }
    }
}
