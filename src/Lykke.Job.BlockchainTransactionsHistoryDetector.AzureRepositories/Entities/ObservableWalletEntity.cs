using Lykke.AzureStorage.Tables;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.AzureRepositories.Entities
{
    public class ObservableWalletEntity : AzureTableEntity
    {
        public string BlockchainType { get; set; }

        public string LatestProcessedHash { get; set; }

        public string WalletAddress { get; set; }

        public string WalletAssetId { get; set; }
    }
}
