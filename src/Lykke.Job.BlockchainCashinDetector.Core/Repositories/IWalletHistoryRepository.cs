using System;
using System.Threading.Tasks;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Domain
{
    public interface IWalletHistoryRepository
    {
        Task<WalletHistoryAggregate> GetOrAddAsync(string blockchainType, string walletAddress, Func<WalletHistoryAggregate> newAggregateFactory);
        Task<WalletHistoryAggregate> GetAsync(Guid operationId);
        Task<WalletHistoryAggregate> TryGetAsync(Guid operationId);
        Task<WalletHistoryAggregate> TryGetAsync(string integrationLayerId, string address, string assetId);
        Task SaveAsync(WalletHistoryAggregate aggregate);
    }
}
