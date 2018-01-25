using System;
using System.Threading.Tasks;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Domain
{
    public interface ILastTransactionRepository
    {
        Task<LastTransaction> GetOrAddAsync(string blockchainType, string walletAddress, Func<LastTransaction> newAggregateFactory);
        Task<LastTransaction> TryGetAsync(string integrationLayerId, string address, WalletAddressType type);
        Task SaveAsync(LastTransaction aggregate);
    }
}
