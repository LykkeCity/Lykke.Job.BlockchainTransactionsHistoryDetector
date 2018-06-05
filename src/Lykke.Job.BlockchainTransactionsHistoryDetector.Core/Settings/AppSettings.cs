using JetBrains.Annotations;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Settings.BlockchainsIntegration;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Settings.Job;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Settings.SlackNotifications;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Settings
{
    [UsedImplicitly]
    public class AppSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public BlockchainsIntegrationSettings BlockchainsIntegration { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public BlockchainTransactionsHistoryDetectorSettings BlockchainTransactionsHistoryDetectorJob { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}
