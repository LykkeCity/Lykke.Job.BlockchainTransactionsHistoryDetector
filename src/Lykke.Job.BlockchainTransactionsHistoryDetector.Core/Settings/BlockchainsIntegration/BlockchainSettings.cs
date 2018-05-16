using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Settings.BlockchainsIntegration
{
    [UsedImplicitly]
    public class BlockchainSettings
    {
        [HttpCheck("/api/isalive", false)]
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string ApiUrl { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string Type { get; set; }
    }
}
