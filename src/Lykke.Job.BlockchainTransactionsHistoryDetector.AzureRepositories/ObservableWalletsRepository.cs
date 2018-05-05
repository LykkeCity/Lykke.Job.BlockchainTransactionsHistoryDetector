using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using AzureStorage.Tables;
using Common;
using Common.Log;
using Lykke.Job.BlockchainTransactionsHistoryDetector.AzureRepositories.Entities;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.DTOs;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Repositories;
using Lykke.SettingsReader;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.AzureRepositories
{
    public class ObservableWalletsRepository : IObservableWalletsRepository
    {
        private readonly INoSQLTableStorage<ObservableWalletEntity> _table;

        private ObservableWalletsRepository(
            INoSQLTableStorage<ObservableWalletEntity> table)
        {
            _table = table;
        }
        
        public static IObservableWalletsRepository Create(IReloadingManager<string> connectionString, ILog log)
        {
            var table = AzureTableStorage<ObservableWalletEntity>.Create
            (
                connectionString,
                "TransactionsHistoryObservableWallets",
                log
            );

            return new ObservableWalletsRepository(table);
        }


        private static string GetPartitionKey(string blockchainType, string walletAddress)
        {
            return $"{blockchainType}-{walletAddress.CalculateHexHash32(3)}";
        }

        private static string GetRowKey(string walletAddress)
        {
            return walletAddress;
        }


        public async Task AddIfNotExistsAsync(string blockchainType, string walletAddress)
        {
            await _table.TryInsertAsync(new ObservableWalletEntity
            {
                BlockchainType = blockchainType,
                WalletAddress = walletAddress,

                PartitionKey = GetPartitionKey(blockchainType, walletAddress),
                RowKey = GetRowKey(walletAddress)
            });
        }

        public async Task DeleteIfExistsAsync(string blockchainType, string walletAddress)
        {
            await _table.DeleteIfExistAsync
            (
                GetPartitionKey(blockchainType, walletAddress),
                GetRowKey(walletAddress)
            );
        }

        public async Task<(IEnumerable<ObservableWalletDto> Wallets, string ContinuationToken)> GetAllAsync(int take, string continuationToken = null)
        {
            IEnumerable<ObservableWalletEntity> entities; 

            (entities, continuationToken) = await _table.GetDataWithContinuationTokenAsync(take, continuationToken);

            var wallets = entities.Select(x => new ObservableWalletDto
            {
                BlockchainType = x.BlockchainType,
                LatestProcessedHash = x.LatestProcessedHash,
                WalletAddress = x.WalletAddress
            });

            return (wallets, continuationToken);
        }

        public async Task UpdateLatestProcessedHashAsync(string blockchainType, string walletAddress, string hash)
        {
            var entity = await _table.GetDataAsync
            (
                GetPartitionKey(blockchainType, walletAddress),
                GetRowKey(walletAddress)
            );

            if (entity != null)
            {
                entity.LatestProcessedHash = hash;

                await _table.InsertOrReplaceAsync(entity);
            }
        }
    }
}
