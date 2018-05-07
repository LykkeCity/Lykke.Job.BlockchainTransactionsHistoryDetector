using System.Threading.Tasks;
using Lykke.SettingsReader;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.WalletsImporter
{
    public class SettingsManager : IReloadingManager<string>
    {
        public SettingsManager(string value)
        {
            CurrentValue = value;
        }

        public async Task<string> Reload()
        {
            throw new System.NotImplementedException();
        }

        public bool HasLoaded
            => true;

        public string CurrentValue { get; }
    }
}
