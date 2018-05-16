using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.SettingsReader;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.WalletsImporter
{
    public class SourceRepository
    {
        private readonly INoSQLTableStorage<SourceWalletEntity> _additionalWalletsTable;
        private readonly INoSQLTableStorage<SourceWalletEntity> _defaultWalletsTable;


        private SourceRepository(
            INoSQLTableStorage<SourceWalletEntity> additionalWalletsTable,
            INoSQLTableStorage<SourceWalletEntity> defaultWalletsTable)
        {
            _additionalWalletsTable = additionalWalletsTable;
            _defaultWalletsTable = defaultWalletsTable;
        }
        
        public static SourceRepository Create(IReloadingManager<string> connectionString, ILog log)
        {
            return new SourceRepository
            (
                additionalWalletsTable: AzureTableStorage<SourceWalletEntity>.Create
                (
                    connectionString,
                    "AdditionalWallets",
                    log
                ),
                defaultWalletsTable: AzureTableStorage<SourceWalletEntity>.Create
                (
                    connectionString,
                    "Wallets",
                    log
                )
            );
        }


        public async Task<IEnumerable<SourceWalletEntity>> GetAllAsync()
        {
            var entities = await Task.WhenAll
            (
                GetAllAsync(_additionalWalletsTable),
                GetAllAsync(_defaultWalletsTable)
            );

            return entities.SelectMany(x => x);
        }

        private async Task<IEnumerable<SourceWalletEntity>> GetAllAsync(INoSQLTableStorage<SourceWalletEntity> table)
        {
            string continuationToken = null;

            var result = new List<SourceWalletEntity>();

            do
            {
                IEnumerable<SourceWalletEntity> entities;
                
                (entities, continuationToken) = await table.GetDataWithContinuationTokenAsync(1000, continuationToken);

                result.AddRange(entities);

            } while (continuationToken != null);

            return result;
        }
    }
}
