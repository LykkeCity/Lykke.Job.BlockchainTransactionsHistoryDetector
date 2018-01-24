using JetBrains.Annotations;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Settings.JobSettings
{
    [UsedImplicitly]
    public class RequestsSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public int BatchSize { get; set; }
    }
}
