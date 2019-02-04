using Autofac;
using Lykke.Common.Log;
using Lykke.Job.BlockchainOperationsExecutor.AzureRepositories.OperationExecutions;
using Lykke.Job.BlockchainOperationsExecutor.AzureRepositories.TransactionExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.OperationExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Core.Domain.TransactionExecutions;
using Lykke.Job.BlockchainOperationsExecutor.Settings.JobSettings;
using Lykke.Job.BlockchainOperationsExecutor.Workflow.CommandHandlers.TransactionExecution;
using Lykke.SettingsReader;

namespace Lykke.Job.BlockchainOperationsExecutor.Modules
{
    public class RepositoriesModule : Module
    {
        private readonly IReloadingManager<DbSettings> _dbSettings;

        public RepositoriesModule(IReloadingManager<DbSettings> dbSettings)
        {
            _dbSettings = dbSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => OperationExecutionsRepository.Create(_dbSettings.Nested(x => x.DataConnString), c.Resolve<ILogFactory>()))
                .As<IOperationExecutionsRepository>()
                .SingleInstance();

            builder.Register(c => ActiveTransactionsRepository.Create(_dbSettings.Nested(x => x.DataConnString), c.Resolve<ILogFactory>()))
                .As<IActiveTransactionsRepository>()
                .SingleInstance();

            builder.Register(c => TransactionExecutionsRepository.Create(_dbSettings.Nested(x => x.DataConnString), c.Resolve<ILogFactory>()))
                .As<ITransactionExecutionsRepository>()
                .SingleInstance();

            builder.Register(c => AddressLocksRepository.Create(_dbSettings.Nested(x => x.DataConnString), c.Resolve<ILogFactory>()))
                .As<IAddressLocksRepository>()
                .SingleInstance();

            builder.Register(c => CommandHandlerEventRepository.Create(_dbSettings.Nested(x => x.DataConnString),
                    c.Resolve<ILogFactory>(),
                    CommandHandlerEventConfigurer.ConfigureCapturedEvents()))
                .As<ITransactionExecutionsRepository>()
                .SingleInstance();
        }
    }
}
