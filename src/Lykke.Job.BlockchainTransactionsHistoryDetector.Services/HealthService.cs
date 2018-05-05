using System.Collections.Generic;
using Lykke.Common.Health;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Services;


namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Services
{
    public class HealthService : IHealthService
    {
        public string GetHealthViolationMessage()
        {
            return null;
        }

        public IEnumerable<HealthIssue> GetHealthIssues()
        {
            var issues = new HealthIssuesCollection();
            

            return issues;
        }
    }
}
