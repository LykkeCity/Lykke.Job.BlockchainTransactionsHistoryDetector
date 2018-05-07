using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.Cache;
using Lykke.Cqrs;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Contract.Events;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.DTOs;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Repositories;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Settings.BlockchainsIntegration;
using Lykke.Service.BlockchainApi.Client;
using Lykke.Service.BlockchainApi.Client.Models;


namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Workflow.Processes
{
    [UsedImplicitly]
    public class NewTransactionDetectionProcess : IProcess
    {
        private readonly OnDemandDataCache<BlockchainAsset> _assetsCache;
        private readonly IDictionary<string, BlockchainApiClient> _blockchainApiClients;
        private readonly string[] _enabledBlockchainTypes;
        private readonly IObservableWalletsRepository _observableWalletsRepository;
        private readonly ILog _log;
        
        private CancellationTokenSource _cancellationTokenSource;
        private IEventPublisher _eventPublisher;
        private Task _task;


        public NewTransactionDetectionProcess(
            BlockchainsIntegrationSettings blockchainsIntegrationSettings,
            IObservableWalletsRepository observableWalletsRepository,
            ILog log)
        {
            _assetsCache = new OnDemandDataCache<BlockchainAsset>();
            _blockchainApiClients = CreateBlockchainApiClients(blockchainsIntegrationSettings, log);
            _enabledBlockchainTypes = GetEnabledBlockchainTypes(blockchainsIntegrationSettings);
            _observableWalletsRepository = observableWalletsRepository;
            _log = log;
        }


        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();

            try
            { 
                _task?.Wait();
            }
            catch (AggregateException)
            {

            }
        }

        public void Start(ICommandSender commandSender, IEventPublisher eventPublisher)
        {
            _cancellationTokenSource = new CancellationTokenSource();
            _eventPublisher = eventPublisher;

            var cancellationToken = _cancellationTokenSource.Token;

            _task = Task.Run(async () =>
            {

                while (!cancellationToken.IsCancellationRequested)
                {
                    await DetectNewTransactionsAsync(cancellationToken);
                }

            }, cancellationToken);
        }

        private static IDictionary<string, BlockchainApiClient> CreateBlockchainApiClients(BlockchainsIntegrationSettings settings, ILog log)
        {
            return settings.Blockchains.ToDictionary
            (
                x => x.Type,
                x => new BlockchainApiClient(log, x.ApiUrl)
            );
        }

        private static string[] GetEnabledBlockchainTypes(BlockchainsIntegrationSettings settings)
        {
            return settings.Blockchains
                .Where(x => x.EnableTransactionsHistory)
                .Select(x => x.Type)
                .ToArray();
        }

        private async Task DetectNewTransactionsAsync(CancellationToken cancellationToken)
        {
            try
            {

                string continuationToken = null;

                do
                {

                    IEnumerable<ObservableWalletDto> wallets;

                    (wallets, continuationToken) = await _observableWalletsRepository.GetAllAsync(_enabledBlockchainTypes, 100, continuationToken);

                    await Task.WhenAll(wallets.Select
                    (
                        x => DetectNewTransactionsForWalletAsync(x, cancellationToken)
                    ));

                } while (continuationToken != null && !cancellationToken.IsCancellationRequested);

                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);

            }
            catch (Exception e)
            {
                await _log.WriteWarningAsync
                (
                    component: nameof(NewTransactionDetectionProcess),
                    process: nameof(DetectNewTransactionsAsync),
                    context: null,
                    info: null,
                    ex: e
                );
            }
        }

        private async Task DetectNewTransactionsForWalletAsync(ObservableWalletDto wallet, CancellationToken cancellationToken)
        {
            try
            {
                if (_blockchainApiClients.TryGetValue(wallet.BlockchainType, out var client))
                {
                    var afterHash = wallet.LatestProcessedHash;
                    bool taskCompleted;

                    do
                    {
                        var transactions = (await client.GetHistoryOfIncomingTransactionsAsync
                        (
                            address: wallet.WalletAddress,
                            afterHash: afterHash,
                            take: 100,
                            assetAccuracyProvider: assetId => GetAssetAccuracy(wallet.BlockchainType, assetId)
                        )).ToList();

                        foreach (var transaction in transactions)
                        {
                            _eventPublisher.PublishEvent(new TransactionDetectedEvent
                            {
                                Amount = transaction.Amount,
                                AssetId = transaction.AssetId,
                                BlockchainType = wallet.BlockchainType,
                                FromAddress = transaction.FromAddress,
                                Hash = transaction.Hash,
                                Timestamp = transaction.Timestamp,
                                ToAddress = transaction.ToAddress,
                                TransactionType = transaction.TransactionType
                            });

                            afterHash = transaction.Hash;
                        }

                        taskCompleted = transactions.Count == 0;

                    } while (!taskCompleted && !cancellationToken.IsCancellationRequested);

                    if (wallet.LatestProcessedHash != afterHash)
                    {
                        await _observableWalletsRepository.UpdateLatestProcessedHashAsync
                        (
                            blockchainType: wallet.BlockchainType,
                            walletAddress: wallet.WalletAddress,
                            walletAssetId: wallet.WalletAssetId,
                            hash: afterHash
                        );
                    }
                }
            }
            catch (Exception e)
            {
                await _log.WriteWarningAsync
                (
                    component: nameof(NewTransactionDetectionProcess),
                    process: nameof(DetectNewTransactionsForWalletAsync),
                    context: null,
                    info: null,
                    ex: e
                );
            }
        }

        private int GetAssetAccuracy(string blockchainType, string assetId)
        {
            var asset = _assetsCache.GetOrAdd
            (
                $"{blockchainType}-{assetId}",
                key => _blockchainApiClients[blockchainType].GetAssetAsync(assetId).Result
            );

            return asset.Accuracy;
        }
    }
}
