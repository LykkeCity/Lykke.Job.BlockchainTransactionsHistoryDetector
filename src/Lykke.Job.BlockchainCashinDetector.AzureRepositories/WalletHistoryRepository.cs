using AzureStorage;
using Lykke.AzureStorage.Tables.Paging;
using Lykke.Job.BlockchainTransactionsHistoryDetector.AzureRepositories.Entities;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Domain;
using Microsoft.WindowsAzure.Storage.Table;
using System;
using System.Collections.Generic;
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
            PagingInfo pagingInfo = new PagingInfo() { };

            var result = await _table.GetDataWithContinuationTokenAsync(TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, blockchainType), null);
        }

        public async Task<WalletHistoryAggregate> GetOrAddAsync(string blockchainType, string walletAddress, Func<WalletHistoryAggregate> newAggregateFactory)
        {
            var walletHistory = await _table.GetDataAsync(WalletHistoryEntity.GetPartitionKey(blockchainType), WalletHistoryEntity.GetRowKey(walletAddress));

            if (walletHistory == null)
            {
                var newAggegate = newAggregateFactory();
                var entity = WalletHistoryEntity.FromDomain(newAggegate);

                await _table.InsertOrReplaceAsync(entity);
            }

            return WalletHistoryEntity.ToDomain(walletHistory);
        }

        public async Task SaveAsync(WalletHistoryAggregate aggregate)
        {
            var entity = WalletHistoryEntity.FromDomain(aggregate);

            await _table.InsertOrReplaceAsync(entity);
        }

        public Task<WalletHistoryAggregate> TryGetAsync(string integrationLayerId, string address, string assetId)
        {
            throw new NotImplementedException();
        }
    }
}
