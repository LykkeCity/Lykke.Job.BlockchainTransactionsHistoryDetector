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
    public class LastTransactionRepository : ILastTransactionRepository
    {
        private readonly INoSQLTableStorage<LastTransactionEntity> _table;

        public LastTransactionRepository(INoSQLTableStorage<LastTransactionEntity> table)
        {
            _table = table;
        }

        public async Task<LastTransaction> GetOrAddAsync(string blockchainType, string walletAddress, Func<LastTransaction> newAggregateFactory)
        {
            var walletHistory = await _table.GetDataAsync(LastTransactionEntity.GetPartitionKey(blockchainType), LastTransactionEntity.GetRowKey(walletAddress));

            if (walletHistory == null)
            {
                var newAggegate = newAggregateFactory();
                var entity = LastTransactionEntity.FromDomain(newAggegate);

                await _table.InsertOrReplaceAsync(entity);
            }

            return LastTransactionEntity.ToDomain(walletHistory);
        }

        public async Task SaveAsync(LastTransaction aggregate)
        {
            var entity = LastTransactionEntity.FromDomain(aggregate);

            await _table.InsertOrReplaceAsync(entity);
        }

        public async Task<LastTransaction> TryGetAsync(string integrationLayerId, string address, WalletAddressType type)
        {
            var lastTransaction = await _table.GetDataAsync(LastTransactionEntity.GetPartitionKey(integrationLayerId), LastTransactionEntity.GetRowKey(address));

            if (lastTransaction == null)
                return null;

            return LastTransactionEntity.ToDomain(lastTransaction);
        }
    }
}
