using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Job.BlockchainTransfersExecutor.Core.Services;
using Lykke.Job.BlockchainTransfersExecutor.Core.Settings.JobSettings;
using Lykke.Job.BlockchainTransfersExecutor.Services;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using BlockchainTransfersExecutorSettings = Lykke.Job.BlockchainTransfersExecutor.Settings.JobSettings.BlockchainTransfersExecutorSettings;
using DbSettings = Lykke.Job.BlockchainTransfersExecutor.Settings.JobSettings.DbSettings;

namespace Lykke.Job.BlockchainTransfersExecutor.Modules
{
    public class JobModule : Module
    {
        private readonly BlockchainTransfersExecutorSettings _settings;
        private readonly IReloadingManager<DbSettings> _dbSettingsManager;
        private readonly ILog _log;
        // NOTE: you can remove it if you don't need to use IServiceCollection extensions to register service specific dependencies
        private readonly IServiceCollection _services;

        public JobModule(BlockchainTransfersExecutorSettings settings, IReloadingManager<DbSettings> dbSettingsManager, ILog log)
        {
            _settings = settings;
            _log = log;
            _dbSettingsManager = dbSettingsManager;

            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
            // NOTE: Do not register entire settings in container, pass necessary settings to services which requires them
            // ex:
            // builder.RegisterType<QuotesPublisher>()
            //  .As<IQuotesPublisher>()
            //  .WithParameter(TypedParameter.From(_settings.Rabbit.ConnectionString))

            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            // TODO: Add your dependencies here

            builder.Populate(_services);
        }

    }
}
