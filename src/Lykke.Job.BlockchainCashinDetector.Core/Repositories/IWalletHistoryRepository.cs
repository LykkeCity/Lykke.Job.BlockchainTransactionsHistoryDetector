using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Domain
{
    public interface IWalletHistoryRepository
    {
        Task<(IEnumerable<WalletHistoryAggregate>, string continuationToken)> EnumerateWalletsAsync(int take, string continuationToken = null);
        Task<(IEnumerable<WalletHistoryAggregate>, string continuationToken)> EnumerateWalletsAsync(string blockchainType, int take, string continuationToken = null);
        Task<WalletHistoryAggregate> GetOrAddAsync(string blockchainType, string walletAddress, Func<WalletHistoryAggregate> newAggregateFactory);
        Task<WalletHistoryAggregate> TryGetAsync(string integrationLayerId, string address, string assetId);
        Task SaveAsync(WalletHistoryAggregate aggregate);
    }
}
