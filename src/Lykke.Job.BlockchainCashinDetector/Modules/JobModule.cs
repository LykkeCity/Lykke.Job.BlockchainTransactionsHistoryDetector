using System;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Common.Log;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Services;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Services;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Settings.Assets;
using Lykke.MatchingEngine.Connector.Services;
using Lykke.Service.Assets.Client;
using Microsoft.Extensions.DependencyInjection;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Modules
{
    public class JobModule : Module
    {
        private readonly AssetsSettings _assetsSettings;
        private readonly ILog _log;
        private readonly ServiceCollection _services;

        public JobModule(AssetsSettings assetsSettings,
            ILog log)
        {
            _assetsSettings = assetsSettings;
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

            _services.RegisterAssetsClient(new AssetServiceSettings
            {
                BaseUri = new Uri(_assetsSettings.ServiceUrl),
                AssetsCacheExpirationPeriod = _assetsSettings.CacheExpirationPeriod,
                AssetPairsCacheExpirationPeriod = _assetsSettings.CacheExpirationPeriod
            });

            builder.Populate(_services);
        }
    }
}
