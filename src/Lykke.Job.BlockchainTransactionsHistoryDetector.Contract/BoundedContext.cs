using JetBrains.Annotations;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Contract
{
    [PublicAPI]
    public class BoundedContext
    {
        public static string Name = "bcn-integration.transactions";

        public static string EventsRoute = "events";
    }
}
