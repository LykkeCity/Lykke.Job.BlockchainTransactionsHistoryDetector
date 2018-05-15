using JetBrains.Annotations;
using Lykke.AzureStorage.Tables;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.WalletsImporter
{
    public class SourceWalletEntity : AzureTableEntity
    {
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string Address { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string AssetId { get; set; }

        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string IntegrationLayerId { get; set; }
    }
}
