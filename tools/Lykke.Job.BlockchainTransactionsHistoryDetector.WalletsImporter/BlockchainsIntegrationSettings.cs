using System.Collections.Generic;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.WalletsImporter
{
    public class BlockchainsIntegrationSettings
    {
        public IEnumerable<BlockchainSettings> Blockchains { get; set; }
    }
}
