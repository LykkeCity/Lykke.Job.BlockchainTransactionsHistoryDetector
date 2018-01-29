using Autofac;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Job.BlockchainTransactionsHistoryDetector.AzureRepositories;
using Lykke.Job.BlockchainTransactionsHistoryDetector.AzureRepositories.Entities;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Domain;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Settings.JobSettings;
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
            _log = log;
            _dbSettings = dbSettings;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.Register(c => new WalletHistoryRepository(AzureTableStorage<WalletHistoryEntity>.Create(_dbSettings.ConnectionString(x => x.DataConnString), 
                "WalletObservation",
                _log)))
                .As<IWalletHistoryRepository>();

            builder.Register(c => new LastTransactionRepository(AzureTableStorage<LastTransactionEntity>.Create(_dbSettings.ConnectionString(x => x.DataConnString),
                "LastTrasnaction",
                _log)))
                .As<ILastTransactionRepository>();
        }
    }
}
