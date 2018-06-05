using JetBrains.Annotations;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Settings.Job
{
    [UsedImplicitly]
    public class BlockchainTransactionsHistoryDetectorSettings
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public CqrsSettings Cqrs { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public DbSettings Db { get; set; }
    }
}
