using Autofac;
using Common.Log;
using Lykke.Job.BlockchainTransactionsHistoryDetector.AzureRepositories;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Repositories;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Settings.Job;
using Lykke.SettingsReader;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Modules
{
    public class RepositoriesModule : Module
    {
        private readonly IReloadingManager<DbSettings> _dbSettings;
        private readonly ILog _log;

        public RepositoriesModule(
            IReloadingManager<DbSettings> dbSettings,
            ILog log)
        {
            _dbSettings = dbSettings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(c => ObservableWalletsRepository.Create(_dbSettings.Nested(x => x.DataConnString), _log))
                .As<IObservableWalletsRepository>()
                .SingleInstance();
        }
    }
}
