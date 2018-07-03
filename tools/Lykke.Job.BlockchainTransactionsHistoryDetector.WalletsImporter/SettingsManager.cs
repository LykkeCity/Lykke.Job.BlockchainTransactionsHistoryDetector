using System;
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

        public Task<string> Reload()
        {
            return Task.FromResult(CurrentValue);
        }

        public bool WasReloadedFrom(DateTime dateTime)
        {
            throw new NotImplementedException();
        }


        public bool HasLoaded
            => true;

        public string CurrentValue { get; }
    }
}
