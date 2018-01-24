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

    public class LastTransactionAggregate
    {
        public string BlockchainType { get; }
        public string Address { get; }
        public string BlockchainAssetId { get; }
        public string TransactionHash { get; private set; }

        private LastTransactionAggregate(
            string blockchainType,
            string address,
            string blockchainAssetId,
            string transactionHash)
        {
            BlockchainType = blockchainType;
            Address = address;
            BlockchainAssetId = blockchainAssetId;
        }

        public static LastTransactionAggregate CreateLatest(string blockchainType, string address, string blockchainAssetId, string blockchainTransactionHash)
        {
            return new LastTransactionAggregate(blockchainType, address, blockchainAssetId, blockchainTransactionHash);
        }
    }
}
