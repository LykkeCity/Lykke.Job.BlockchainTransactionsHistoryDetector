using System.Threading.Tasks;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Services
{
    public interface IShutdownManager
    {
        Task StopAsync();
    }
}
