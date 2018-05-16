using JetBrains.Annotations;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Settings.Job
{
    [UsedImplicitly]
    public class CqrsSettings
    {
        [AmqpCheck]
        [UsedImplicitly(ImplicitUseKindFlags.Assign)]
        public string RabbitConnectionString { get; set; }
    }
}
