namespace Lykke.Job.BlockchainTransactionsHistoryDetector.WalletsImporter
{
    public class SourceSettings
    {
        public BlockchainsIntegrationSettings BlockchainsIntegration { get; set; }
        
        public BlockchainWalletsServiceSettings BlockchainWalletsService { get; set; }
    }
}
