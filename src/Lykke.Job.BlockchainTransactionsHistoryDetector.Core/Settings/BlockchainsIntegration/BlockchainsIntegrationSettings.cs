using System.Collections.Generic;
using JetBrains.Annotations;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Settings.BlockchainsIntegration
{
    [UsedImplicitly]
    public class BlockchainsIntegrationSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public IReadOnlyList<BlockchainSettings> Blockchains { get; set; }
    }
}
