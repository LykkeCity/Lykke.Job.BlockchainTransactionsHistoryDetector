using Common;
using Lykke.AzureStorage.Tables;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.AzureRepositories.Entities
{
    public class LastTransactionEntity : AzureTableEntity
    {
        #region Fields

        // ReSharper disable MemberCanBePrivate.Global

        public string BlockchainType { get; set; }
        public string Address { get; set; }
        public string TransactionHash { get; set; }
        public WalletAddressType WalletAddressType { get; set; }

        // ReSharper restore MemberCanBePrivate.Global

        #endregion

        #region Keys

        public static string GetPartitionKey(string blockchainType)
        {
            return $"{blockchainType}".CalculateHexHash32(3);
        }

        public static string GetRowKey(string walletAddress)
        {
            return $"{walletAddress.ToLower()}.";
        }

        #endregion

        #region Conversion

        public static LastTransactionEntity FromDomain(LastTransaction aggregate)
        {
            return new LastTransactionEntity
            {
                PartitionKey = GetPartitionKey(aggregate.BlockchainType),
                RowKey = GetRowKey(aggregate.Address),
                Address = aggregate.Address,
                BlockchainType = aggregate.BlockchainType,
                TransactionHash = aggregate.TransactionHash,
                WalletAddressType = aggregate.WalletAddressType
            };
        }

        public static LastTransaction ToDomain(LastTransactionEntity entity)
        {
            return LastTransaction.Create(entity.BlockchainType, entity.Address, entity.TransactionHash, entity.WalletAddressType);
        }

        #endregion
    }
}
