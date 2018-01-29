using AzureStorage;
using Lykke.AzureStorage.Tables.Paging;
using Lykke.Job.BlockchainTransactionsHistoryDetector.AzureRepositories.Entities;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Domain;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.AzureRepositories
{
    public class WalletHistoryRepository : IWalletHistoryRepository
    {
        private readonly INoSQLTableStorage<WalletHistoryEntity> _table;

        public WalletHistoryRepository(INoSQLTableStorage<WalletHistoryEntity> table)
        {
            _table = table;
        }

        public Task<(IEnumerable<WalletHistoryAggregate>, string continuationToken)> EnumerateWalletsAsync(int take, string continuationToken = null)
        {
            throw new NotImplementedException();
        }

        public async Task<(IEnumerable<WalletHistoryAggregate>, string continuationToken)> EnumerateWalletsAsync(string blockchainType, int take, string continuationToken = null)
        {
            var newQuery = new TableQuery<WalletHistoryEntity>()
            {
                FilterString = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, WalletHistoryEntity.GetPartitionKey(blockchainType)),
                TakeCount = take
            };

            var (collection, newToken) = await
                _table.GetDataWithContinuationTokenAsync(newQuery, continuationToken);
            var mappedCollection = collection.Select(x => WalletHistoryEntity.ToDomain(x));

            return (mappedCollection, newToken);
        }

        public async Task<WalletHistoryAggregate> GetOrAddAsync(string blockchainType, string walletAddress, Func<WalletHistoryAggregate> newAggregateFactory)
        {
            var walletHistory = await _table.GetDataAsync(WalletHistoryEntity.GetPartitionKey(blockchainType), WalletHistoryEntity.GetRowKey(walletAddress));

            if (walletHistory == null)
            {
                var newAggegate = newAggregateFactory();
                walletHistory = WalletHistoryEntity.FromDomain(newAggegate);

                await _table.InsertOrReplaceAsync(walletHistory);
            }

            return WalletHistoryEntity.ToDomain(walletHistory);
        }

        public async Task SaveAsync(WalletHistoryAggregate aggregate)
        {
            var entity = WalletHistoryEntity.FromDomain(aggregate);

            await _table.InsertOrReplaceAsync(entity);
        }

        public async Task<WalletHistoryAggregate> TryGetAsync(string integrationLayerId, string address, string assetId)
        {
            var walletHistory = await _table.GetDataAsync(WalletHistoryEntity.GetPartitionKey(integrationLayerId), WalletHistoryEntity.GetRowKey(address));

            if (walletHistory == null)
                return null;

            return WalletHistoryEntity.ToDomain(walletHistory);
        }
    }
}
