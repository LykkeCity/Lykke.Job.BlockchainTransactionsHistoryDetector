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

    public class WalletHistoryAggregate
    {
        public DateTime CreationMoment { get; }
        public Guid AggregateId { get; }
        public string BlockchainType { get; }
        public string WalletAddress { get; }
        public string AssetId { get; }
        public WalletAddressType WalletAddressType { get; }
        public WalletHistoryState WalletHistoryState { get; }

        private WalletHistoryAggregate(
            string blockchainType,
            string walletAddress,
            string assetId,
            WalletAddressType walletAddressType)
        {
            CreationMoment = DateTime.UtcNow;
            AggregateId = Guid.NewGuid();
            BlockchainType = blockchainType;
            WalletAddress = walletAddress;
            WalletAddressType = walletAddressType;
            WalletHistoryState = WalletHistoryState.Started;
            AssetId = assetId;
        }

        private WalletHistoryAggregate(
            Guid aggregateId,
            string blockchainType,
            string walletAddress,
            string assetId,
            WalletAddressType walletAddressType,
            WalletHistoryState walletHistoryState)
        {
            CreationMoment = DateTime.UtcNow;
            AggregateId = aggregateId;
            BlockchainType = blockchainType;
            WalletAddress = walletAddress;
            WalletAddressType = walletAddressType;
            WalletHistoryState = walletHistoryState;
            AssetId = assetId;
        }

        public static WalletHistoryAggregate CreateNew(
            string blockchainType,
            string walletAddress,
            string assetId,
            WalletAddressType walletAddressType)
        {
            return new WalletHistoryAggregate(blockchainType, walletAddress, assetId, walletAddressType);
        }

        public static WalletHistoryAggregate StopObservation(
            Guid aggregatedId,
            string blockchainType,
            string walletAddress,
            string assetId,
            WalletAddressType walletAddressType)
        {
            return new WalletHistoryAggregate(aggregatedId, blockchainType, walletAddress, assetId, walletAddressType, WalletHistoryState.Stopped);
        }
    }
}
