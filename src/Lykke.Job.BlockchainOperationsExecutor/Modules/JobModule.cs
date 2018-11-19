using System;
using Autofac;
using Lykke.Common.Chaos;
using Lykke.Job.BlockchainOperationsExecutor.AppServices.Health;
using Lykke.Job.BlockchainOperationsExecutor.AppServices.Lifecycle;
using Lykke.Job.BlockchainOperationsExecutor.Settings.Assets;
using Lykke.Service.Assets.Client;

namespace Lykke.Job.BlockchainOperationsExecutor.Modules
{
    public class JobModule : Module
    {
        private readonly AssetsSettings _assetsSettings;
        private readonly ChaosSettings _chaosSettings;

        public JobModule(AssetsSettings assetsSettings, ChaosSettings chaosSettings)
        {
            _assetsSettings = assetsSettings;
            _chaosSettings = chaosSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();

            builder.RegisterAssetsClient
            (
                new AssetServiceSettings
                {
                    BaseUri = new Uri(_assetsSettings.ServiceUrl),
                    AssetsCacheExpirationPeriod = _assetsSettings.CacheExpirationPeriod,
                    AssetPairsCacheExpirationPeriod = _assetsSettings.CacheExpirationPeriod
                }
            );

            builder.RegisterChaosKitty(_chaosSettings);
        }
    }
}
