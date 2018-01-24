using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Domain;
using MessagePack;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Workflow.Commands
{
    /// <summary>
    /// Command to detect active positive balance on the deposit wallet
    /// </summary>
    [MessagePackObject]
    public class MonitoringTransactionHistoryCommand
    {
        [Key(0)]
        public string BlockchainType { get; set; }
        [Key(1)]
        public string WalletAddress { get; set; }
        [Key(2)]
        public WalletAddressType WalletAddressType { get; set; }
    }
}
