using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Contract;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Domain;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Services.BLockchains;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Workflow.Commands;
using Lykke.Service.BlockchainApi.Client;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Workflow.PeriodicalHandlers
{
    public class TransactionHistoryProcessingPeriodicalHandler : TimerPeriod
    {
        private readonly int _batchSize;
        private readonly string _blockchainType;
        private readonly IBlockchainApiClient _blockchainApiClient;
        private readonly ICqrsEngine _cqrsEngine;
        private readonly IWalletHistoryRepository _walletHistoryRepository;

        public TransactionHistoryProcessingPeriodicalHandler(
            ILog log,
            TimeSpan period,
            int batchSize,
            string blockchainType,
            IBlockchainApiClientProvider blockchainApiClientProvider,
            ICqrsEngine cqrsEngine,
            IWalletHistoryRepository walletHistoryRepository) :

            base(
                nameof(TransactionHistoryProcessingPeriodicalHandler),
                (int)period.TotalMilliseconds,
                log.CreateComponentScope($"{nameof(TransactionHistoryProcessingPeriodicalHandler)} : {blockchainType}"))
        {
            _batchSize = batchSize;
            _blockchainType = blockchainType;
            _blockchainApiClient = blockchainApiClientProvider.Get(blockchainType);
            _cqrsEngine = cqrsEngine;
            _walletHistoryRepository = walletHistoryRepository;
        }

        public override async Task Execute()
        {
            var stopwatch = Stopwatch.StartNew();
            var wallets = new HashSet<string>();
            string continuationToken = null;
            bool keepProcessing = true;

            do
            {
                var (addresses, newContinuationToken) = await _walletHistoryRepository.EnumerateWalletsAsync(_blockchainType, _batchSize, continuationToken);
                continuationToken = newContinuationToken;

                foreach (var address in addresses)
                {
                    if (address.WalletHistoryState == WalletHistoryState.Stopped)
                        continue;

                    _cqrsEngine.SendCommand<MonitoringTransactionHistoryCommand>(new MonitoringTransactionHistoryCommand()
                    {
                        BlockchainAssetId = address.AssetId,
                        BlockchainType = _blockchainType,
                        WalletAddress = address.WalletAddress,
                        WalletAddressType = address.WalletAddressType
                    },
                    BoundedContext.BlockChainTransactionHistoryDetectorContext,
                    BoundedContext.BlockChainTransactionHistoryDetectorContext);
                }

                keepProcessing = addresses.Count() != 0;
            } while (keepProcessing);

            await this.Log.WriteInfoAsync(nameof(TransactionHistoryProcessingPeriodicalHandler), _blockchainType, $"Ellapsed: {stopwatch.ElapsedMilliseconds} ms");
        }
    }
}
