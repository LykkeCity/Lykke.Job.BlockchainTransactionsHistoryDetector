﻿using System.Collections.Generic;
using JetBrains.Annotations;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Domain.Health;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Services;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Services
{
    // NOTE: See https://lykkex.atlassian.net/wiki/spaces/LKEWALLET/pages/35755585/Add+your+app+to+Monitoring
    [UsedImplicitly]
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
