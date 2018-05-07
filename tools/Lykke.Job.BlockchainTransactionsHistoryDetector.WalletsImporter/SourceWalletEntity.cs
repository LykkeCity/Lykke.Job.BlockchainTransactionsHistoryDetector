using Lykke.AzureStorage.Tables;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.WalletsImporter
{
    public class SourceWalletEntity : AzureTableEntity
    {
        public string Address { get; set; }

        public string AssetId { get; set; }

        public string IntegrationLayerId { get; set; }
    }
}
