using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Common.Chaos;
using Lykke.Job.BlockchainOperationsExecutor.Core.Services;
using Lykke.Job.BlockchainOperationsExecutor.Services;
using Lykke.Job.BlockchainOperationsExecutor.Settings.Assets;
using Lykke.Service.Assets.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Job.BlockchainOperationsExecutor.Modules
{
    public class JobModule : Module
    {
        private readonly AssetsSettings _assetsSettings;
        private readonly ChaosSettings _chaosSettings;
        private readonly ILog _log;
        private readonly ServiceCollection _services;

        public JobModule(
            AssetsSettings assetsSettings,
            ChaosSettings chaosSettings,
            ILog log)
        {
            _assetsSettings = assetsSettings;
            _chaosSettings = chaosSettings;
            _log = log;
            _services = new ServiceCollection();
        }

        protected override void Load(ContainerBuilder builder)
        {
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

            _services.RegisterAssetsClient
            (
                new AssetServiceSettings
                {
                    BaseUri = new Uri(_assetsSettings.ServiceUrl),
                    AssetsCacheExpirationPeriod = _assetsSettings.CacheExpirationPeriod,
                    AssetPairsCacheExpirationPeriod = _assetsSettings.CacheExpirationPeriod
                },
                _log
            );

            builder.RegisterChaosKitty(_chaosSettings);

            builder.Populate(_services);
        }
    }
}
