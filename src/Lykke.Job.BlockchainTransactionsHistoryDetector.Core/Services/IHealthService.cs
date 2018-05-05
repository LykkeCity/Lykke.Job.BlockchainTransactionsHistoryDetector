using System.Collections.Generic;
using Lykke.Common.Health;


namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Services
{
    public interface IHealthService
    {
        string GetHealthViolationMessage();

        IEnumerable<HealthIssue> GetHealthIssues();
    }
}
