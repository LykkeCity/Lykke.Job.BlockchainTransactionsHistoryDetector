using System;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Domain
{
    [Flags]
    public enum WalletAddressType
    {
        From = 1,
        To = 2,
        Both =  From & To //3
    }
}
