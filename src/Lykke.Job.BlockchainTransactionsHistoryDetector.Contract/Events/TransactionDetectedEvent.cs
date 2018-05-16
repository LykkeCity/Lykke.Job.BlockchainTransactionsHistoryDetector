using System;
using JetBrains.Annotations;
using Lykke.Service.BlockchainApi.Contract.Transactions;
using MessagePack;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Contract.Events
{
    [MessagePackObject]
    public class TransactionDetectedEvent
    {
        /// <summary>
        /// Amount without fee
        /// </summary>
        [Key(0)]
        [UsedImplicitly(ImplicitUseKindFlags.Access)]
        public decimal Amount { get; set; }

        /// <summary>
        /// Asset ID
        /// </summary>
        [Key(1)]
        [UsedImplicitly(ImplicitUseKindFlags.Access)]
        public string AssetId { get; set; }

        /// <summary>
        /// Blockchain type.
        /// </summary>
        [Key(2)]
        [UsedImplicitly(ImplicitUseKindFlags.Access)]
        public string BlockchainType { get; set; }

        /// <summary>
        /// Source address
        /// </summary>
        [Key(3)]
        [UsedImplicitly(ImplicitUseKindFlags.Access)]
        public string FromAddress { get; set; }

        /// <summary>
        /// Transaction hash as base64 string.
        /// </summary>
        [Key(4)]
        [UsedImplicitly(ImplicitUseKindFlags.Access)]
        public string Hash { get; set; }

        /// <summary>
        /// Transaction moment in UTC
        /// </summary>
        [Key(5)]
        [UsedImplicitly(ImplicitUseKindFlags.Access)]
        public DateTime Timestamp { get; set; }

        /// <summary>
        /// Destination address
        /// </summary>
        [Key(6)]
        [UsedImplicitly(ImplicitUseKindFlags.Access)]
        public string ToAddress { get; set; }

        /// <summary>
        /// Type of the transaction.
        /// Can be empty.
        /// </summary>
        [Key(7)]
        [UsedImplicitly(ImplicitUseKindFlags.Access)]
        public TransactionType? TransactionType { get; set; }
    }
}
