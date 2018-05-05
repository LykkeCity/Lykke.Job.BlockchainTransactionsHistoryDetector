using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.DTOs;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Repositories
{
    public interface IObservableWalletsRepository
    {
        Task AddIfNotExistsAsync(string blockchainType, string walletAddress);

        Task DeleteIfExistsAsync(string blockchainType, string walletAddress);
        
        Task<(IEnumerable<ObservableWalletDto> Wallets, string ContinuationToken)> GetAllAsync(int take, string continuationToken = null);
        
        Task UpdateLatestProcessedHashAsync(string blockchainType, string walletAddress, string hash);
    }
}
