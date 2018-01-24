namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Services.BLockchains
{
    public interface IHotWalletsProvider
    {
        string GetHotWalletAddress(string blockchainType);
    }
}
