using System.Collections.Generic;
using System.Threading.Tasks;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.DTOs;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Repositories
{
    public interface IObservableWalletsRepository
    {
        Task AddIfNotExistsAsync(string blockchainType, string walletAddress, string walletAssedId);

        Task DeleteIfExistsAsync(string blockchainType, string walletAddress, string walletAssedId);
        
        Task<(IEnumerable<ObservableWalletDto> Wallets, string ContinuationToken)> GetAllAsync(ICollection<string> blockchainTypes, int take, string continuationToken = null);
        
        Task UpdateLatestProcessedHashAsync(string blockchainType, string walletAddress, string walletAssetId, string hash);
    }
}
