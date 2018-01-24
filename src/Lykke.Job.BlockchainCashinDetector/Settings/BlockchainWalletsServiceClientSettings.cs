using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Settings
{
    [UsedImplicitly]
    public class BlockchainWalletsServiceClientSettings
    {
        [HttpCheck("/api/isalive")]
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string ServiceUrl { get; set; }
    }
}
