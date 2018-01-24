using System.Threading.Tasks;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Services
{
    public interface IStartupManager
    {
        Task StartAsync();
    }
}
