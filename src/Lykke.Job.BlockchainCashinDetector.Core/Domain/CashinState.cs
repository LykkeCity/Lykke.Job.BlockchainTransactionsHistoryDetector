namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Domain
{
    public enum CashinState
    {
        Starting,
        Started,
        EnrolledToMatchingEnging,
        OperationIsFinished,
        MatchingEngineDeduplicationLockIsRemoved
    }
}
