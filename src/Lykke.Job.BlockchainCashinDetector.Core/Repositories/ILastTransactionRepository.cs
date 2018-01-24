using System;
using System.Threading.Tasks;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Domain
{
    public interface ILastTransactionRepository
    {
        Task<LastTransactionAggregate> GetOrAddAsync(string blockchainType, string walletAddress, Func<LastTransactionAggregate> newAggregateFactory);
        Task<LastTransactionAggregate> TryGetAsync(string integrationLayerId, string address);
        Task SaveAsync(LastTransactionAggregate aggregate);
    }
}
