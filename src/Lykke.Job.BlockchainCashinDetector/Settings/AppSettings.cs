using JetBrains.Annotations;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Settings.Assets;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Settings.Blockchain;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Settings.JobSettings;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Settings.SlackNotifications;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Settings
{
    [UsedImplicitly]
    public class AppSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public BlockchainTransactionsHistoryDetectorSettings BlockchainTransactionsHistoryDetectorSettings { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public SlackNotificationsSettings SlackNotifications { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public BlockchainsIntegrationSettings BlockchainsIntegration { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public AssetsSettings Assets { get; set; }
    }
}
