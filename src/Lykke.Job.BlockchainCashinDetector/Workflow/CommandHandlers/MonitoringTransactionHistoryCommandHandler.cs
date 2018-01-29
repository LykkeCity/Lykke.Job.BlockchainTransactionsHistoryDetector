using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Contract.Events;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Domain;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Services.BLockchains;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Workflow.Commands;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.BlockchainApi.Client.Models;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Workflow.CommandHandlers
{
    [UsedImplicitly]
    public class MonitoringTransactionHistoryCommandHandler
    {
        private int _batchSize = 100;
        private readonly ILog _log;
        private readonly ILastTransactionRepository _lastTransactionRepository;
        private readonly IBlockchainApiClientProvider _apiClientProvider;
        private readonly IAssetsServiceWithCache _assetsServiceWithCache;

        //private readonly IHotWalletsProvider _hotWalletsProvider;

        public MonitoringTransactionHistoryCommandHandler(
            ILog log,
            ILastTransactionRepository lastTransactionRepository, 
            IBlockchainApiClientProvider apiClientProvider, 
            Lykke.Service.Assets.Client.IAssetsServiceWithCache assetsServiceWithCache)
        {
            _log = log;
            _lastTransactionRepository = lastTransactionRepository;
            _apiClientProvider = apiClientProvider;
            _assetsServiceWithCache = assetsServiceWithCache;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(MonitoringTransactionHistoryCommand command, IEventPublisher publisher)
        {
#if DEBUG
            _log.WriteInfo(nameof(MonitoringTransactionHistoryCommand), command, "");
#endif
            try
            {
                var allAssets = await _assetsServiceWithCache.GetAllAssetsAsync();
                Dictionary<(string, string), Asset> assetDictionary = allAssets.Where(x => !string.IsNullOrEmpty(x.BlockchainIntegrationLayerAssetId) &&
                                                      !string.IsNullOrEmpty(x.BlockchainIntegrationLayerId))
                    .ToDictionary(x => (x.BlockchainIntegrationLayerId, x.BlockchainIntegrationLayerAssetId));
                var client = _apiClientProvider.Get(command.BlockchainType);
                if (client == null)
                {
                    _log.WriteInfo(nameof(MonitoringTransactionHistoryCommand), command, $"No client registered for blockchain of type {command.BlockchainType}");
                }

                List<HistoricalTransaction> historicalList;
                string cachedLastFromHash = null;
                string cachedLastToHash = null;

                do
                {
                    historicalList = new List<HistoricalTransaction>(_batchSize);
                    IEnumerable<HistoricalTransaction> historicalTransactionFrom = null;
                    IEnumerable<HistoricalTransaction> historicalTransactionTo =  null;

                    if ((command.WalletAddressType & WalletAddressType.From) == WalletAddressType.From)
                    {
                        cachedLastFromHash = string.IsNullOrEmpty(cachedLastFromHash) ? 
                            (await _lastTransactionRepository.TryGetAsync(command.BlockchainType, command.WalletAddress, WalletAddressType.From))?.TransactionHash 
                            : cachedLastFromHash;
                        historicalTransactionFrom = await client.GetHistoryOfOutgoingTransactionsAsync(command.WalletAddress, 
                            cachedLastFromHash ?? "",
                            _batchSize,
                            GetAssetAccuracyFunc(assetDictionary));

                        historicalList.AddRange(historicalTransactionFrom);
                    }

                    if ((command.WalletAddressType & WalletAddressType.To) == WalletAddressType.To)
                    {
                        cachedLastToHash = string.IsNullOrEmpty(cachedLastToHash) ?
                            (await _lastTransactionRepository.TryGetAsync(command.BlockchainType, command.WalletAddress, WalletAddressType.To))?.TransactionHash
                            : cachedLastToHash;
                        historicalTransactionTo = await client.GetHistoryOfIncomingTransactionsAsync(command.WalletAddress,
                            cachedLastToHash ?? "", 
                            _batchSize,
                            GetAssetAccuracyFunc(assetDictionary));

                        historicalList.AddRange(historicalTransactionTo);
                    }

                    foreach (var item in historicalList)
                    {
                        var @event = new TransactionHistoryEvent();
                        publisher.PublishEvent(@event);
                    }

                    if (historicalTransactionFrom != null && historicalTransactionFrom.Count() != 0)
                    {
                        var lastTransactionInBatch = historicalTransactionFrom.Last();
                        await _lastTransactionRepository.SaveAsync(LastTransaction.Create(command.BlockchainType,
                            command.WalletAddress, 
                            lastTransactionInBatch.Hash,
                            WalletAddressType.From));
                    }

                    if (historicalTransactionTo != null && historicalTransactionTo.Count() != 0)
                    {
                        var lastTransactionInBatch = historicalTransactionTo.Last();
                        await _lastTransactionRepository.SaveAsync(LastTransaction.Create(command.BlockchainType,
                            command.WalletAddress,
                            lastTransactionInBatch.Hash,
                            WalletAddressType.To));
                    }

                } while (historicalList.Count >= _batchSize);
            }
            catch (Exception e)
            {
                _log.WriteError(nameof(MonitoringTransactionHistoryCommand), command, e);
                return CommandHandlingResult.Fail(TimeSpan.FromSeconds(30));
            }

            return CommandHandlingResult.Ok();

            Func<string, int> GetAssetAccuracyFunc(Dictionary<(string, string), Asset> assetDictionary)
            {
                var closureDict = assetDictionary;

                return (assetId) =>
                {
                    Asset asset = null;
                    closureDict.TryGetValue((command.BlockchainType, assetId), out asset);

                    return asset != null ? asset.Accuracy : 0;
                };
            }
        }
    }
}
