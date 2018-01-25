using Lykke.AzureStorage.Tables;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.AzureRepositories.Entities
{
    public class WalletHistoryEntity : AzureTableEntity
    {
        #region Fields

        // ReSharper disable MemberCanBePrivate.Global

        public DateTime CreationMoment { get; set; }
        public string BlockchainType { get; set; }
        public string WalletAddress { get; set; }
        public string AssetId { get; set; }
        public WalletAddressType WalletAddressType { get; set; }
        public WalletHistoryState WalletHistoryState { get; set; }

        // ReSharper restore MemberCanBePrivate.Global

        #endregion

        #region Keys

        public static string GetPartitionKey(string blockchainType)
        {
            return $"{blockchainType}";
        }

        public static string GetRowKey(string walletAddress)
        {
            return $"{walletAddress.ToLower()}.";
        }

        #endregion

        #region Conversion

        public static WalletHistoryEntity FromDomain(WalletHistoryAggregate aggregate)
        {
            return new WalletHistoryEntity
            {
                PartitionKey = GetPartitionKey(aggregate.BlockchainType),
                RowKey = GetRowKey(aggregate.WalletAddress),
                AssetId = aggregate.AssetId,
                WalletAddress = aggregate.WalletAddress,
                BlockchainType = aggregate.BlockchainType,
                CreationMoment = aggregate.CreationMoment,
                WalletAddressType = aggregate.WalletAddressType,
                WalletHistoryState = aggregate.WalletHistoryState
            };
        }

        public static WalletHistoryAggregate ToDomain(WalletHistoryEntity entity)
        {
            return WalletHistoryAggregate.Restore(entity.AssetId, 
                entity.BlockchainType, 
                entity.WalletAddress, 
                entity.WalletAddressType, 
                entity.WalletHistoryState);
        }

        #endregion
    }
}
