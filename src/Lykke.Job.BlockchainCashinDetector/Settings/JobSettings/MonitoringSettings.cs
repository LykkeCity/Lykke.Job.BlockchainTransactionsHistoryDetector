using System;
using JetBrains.Annotations;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Settings.JobSettings
{
    [UsedImplicitly]
    public class MonitoringSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public TimeSpan Period { get; set; }
    }
}
