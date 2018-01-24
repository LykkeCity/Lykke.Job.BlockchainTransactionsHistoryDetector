using JetBrains.Annotations;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Settings.JobSettings
{
    [UsedImplicitly]
    public class DbSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string LogsConnString { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string DataConnString { get; set; }
    }
}
