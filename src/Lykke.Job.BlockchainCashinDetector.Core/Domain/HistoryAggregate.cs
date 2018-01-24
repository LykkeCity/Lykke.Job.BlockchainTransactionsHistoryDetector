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

    public class HistoryAggregate
    {
        public string Version { get; }

        public DateTime TimeStamp { get; }

        public Guid OperationId { get; }
        public string BlockchainType { get; }
        public string FromAddress { get; }
        public string ToAddress { get; }
        public string BlockchainAssetId { get; }
        public decimal Amount { get; }
        public string AssetId { get; private set; }
        public string TransactionHash { get; private set; }

        private HistoryAggregate(
            Guid operationId,
            DateTime timeStamp,
            string blockchainType,
            string fromAddress,
            string toAddress,
            string blockchainAssetId,
            decimal amount)
        {
            TimeStamp = TimeStamp;
            OperationId = operationId;
            BlockchainType = blockchainType;
            FromAddress = fromAddress;
            ToAddress = toAddress;
            BlockchainAssetId = blockchainAssetId;
            Amount = amount;
        }
    }
}
