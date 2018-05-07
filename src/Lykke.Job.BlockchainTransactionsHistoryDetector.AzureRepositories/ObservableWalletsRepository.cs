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
using Microsoft.WindowsAzure.Storage.Table;

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

        private static string GetRowKey(string walletAddress, string walletAssetId)
        {
            return $"{walletAddress}-{walletAssetId}";
        }


        public async Task AddIfNotExistsAsync(string blockchainType, string walletAddress, string walletAssetId)
        {
            await _table.TryInsertAsync(new ObservableWalletEntity
            {
                BlockchainType = blockchainType,
                WalletAddress = walletAddress,
                WalletAssetId = walletAssetId,

                PartitionKey = GetPartitionKey(blockchainType, walletAddress),
                RowKey = GetRowKey(walletAddress, walletAssetId)
            });
        }

        public async Task DeleteIfExistsAsync(string blockchainType, string walletAddress, string walletAssetId)
        {
            await _table.DeleteIfExistAsync
            (
                GetPartitionKey(blockchainType, walletAddress),
                GetRowKey(walletAddress, walletAssetId)
            );
        }

        public async Task<(IEnumerable<ObservableWalletDto> Wallets, string ContinuationToken)> GetAllAsync(string[] blockchainTypes, int take, string continuationToken = null)
        {
            if (!blockchainTypes.Any())
            {
                return (new List<ObservableWalletDto>(), null);
            }

            string CreateCondition(string blockchainType)
            {
                return TableQuery.GenerateFilterCondition
                (
                    "BlockchainType",
                    QueryComparisons.Equal,
                    blockchainType
                );
            }

            string AggregateConditions(string current, string blockchainTypeCondition)
            {
                return string.IsNullOrEmpty(current)
                    ? blockchainTypeCondition
                    : TableQuery.CombineFilters(current, TableOperators.Or, blockchainTypeCondition);
            }

            var query = new TableQuery<ObservableWalletEntity>().Where
            (
                blockchainTypes
                    .Select(CreateCondition)
                    .Aggregate(AggregateConditions)
            );
            

            IEnumerable<ObservableWalletEntity> entities; 
            
            (entities, continuationToken) = await _table.GetDataWithContinuationTokenAsync(query, take, continuationToken);

            var wallets = entities.Select(x => new ObservableWalletDto
            {
                BlockchainType = x.BlockchainType,
                LatestProcessedHash = x.LatestProcessedHash,
                WalletAddress = x.WalletAddress,
                WalletAssetId = x.WalletAssetId
            });

            return (wallets, continuationToken);
        }

        public async Task UpdateLatestProcessedHashAsync(string blockchainType, string walletAddress, string walletAssetId, string hash)
        {
            var entity = await _table.GetDataAsync
            (
                GetPartitionKey(blockchainType, walletAddress),
                GetRowKey(walletAddress, walletAssetId)
            );

            if (entity != null)
            {
                entity.LatestProcessedHash = hash;

                await _table.InsertOrReplaceAsync(entity);
            }
        }
    }
}
