using Lykke.Service.BlockchainApi.Client;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Services.BLockchains
{
    public interface IBlockchainApiClientProvider
    {
        IBlockchainApiClient Get(string blockchainType);
    }
}
