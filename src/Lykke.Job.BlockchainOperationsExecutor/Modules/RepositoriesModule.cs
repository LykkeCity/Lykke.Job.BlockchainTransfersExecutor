using Autofac;
using Common.Log;
using Lykke.Job.BlockchainOperationsExecutor.AzureRepositories.OperationExecutions;
using Lykke.Job.BlockchainOperationsExecutor.AzureRepositories.TransactionExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.OperationExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
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
                .As<IOperationExecutionsRepository>()
                .SingleInstance();

            builder.Register(c => ActiveTransactionsRepository.Create(_dbSettings.Nested(x => x.DataConnString), _log))
                .As<IActiveTransactionsRepository>()
                .SingleInstance();

            builder.Register(c => TransactionExecutionsRepository.Create(_dbSettings.Nested(x => x.DataConnString), _log))
                .As<ITransactionExecutionsRepository>()
                .SingleInstance();

            builder.Register(c => SourceAddressLocksRepository.Create(_dbSettings.Nested(x => x.DataConnString), _log))
                .As<ISourceAddresLocksRepoistory>()
                .SingleInstance();
        }
    }
}
