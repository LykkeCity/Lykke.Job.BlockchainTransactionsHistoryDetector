namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Core.DTOs
{
    public class ObservableWalletDto
    {
        public string BlockchainType { get; set; }

        public string LatestProcessedHash { get; set; }

        public string WalletAddress { get; set; }
    }
}
