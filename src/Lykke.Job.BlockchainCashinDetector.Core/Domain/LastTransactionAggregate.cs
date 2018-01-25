using System;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Domain
{

    /*
        {
            // Lykke unique operation ID.
            // Can be empty.
            // Should be not empty for transactions that
            // broadcasted using this Blockchain.Api
            “operationId”: “guid”,
 
            // Transaction moment as ISO 8601 in UTC
            “timestamp”: “datetime”,
 
            // Source address
            “fromAddress”: “string”,
 
            // Destination address
            “toAddress”: “string”,
 
            // Asset ID
            “assetId”: “string”
 
            // Amount without fee. Is integer as string, aligned 
            // to the asset accuracy. Actual value can be 
            // calculated as 
            // x = sourceAmount * (10 ^ asset.Accuracy)
            “amount”: “string”,
 
            // Transaction hash as base64 string
            “hash”: “string”
        }
    */

    public class LastTransaction
    {
        public string BlockchainType { get; }
        public string Address { get; }
        public string TransactionHash { get; }
        public WalletAddressType WalletAddressType { get; }

        private LastTransaction(
            string blockchainType,
            string address,
            string transactionHash,
            WalletAddressType walletAddressType)
        {
            BlockchainType = blockchainType;
            Address = address;
            TransactionHash = transactionHash;
            WalletAddressType = walletAddressType;
        }

        public static LastTransaction CreateLatest(string blockchainType, string address, string blockchainTransactionHash, WalletAddressType walletAddressType)
        {
            return new LastTransaction(blockchainType, address, blockchainTransactionHash, walletAddressType);
        }
    }
}
