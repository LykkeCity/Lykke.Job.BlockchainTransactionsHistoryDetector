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
    public class StartAddressObservationCommandHandler
    {
        private int _batchSize = 100;
        private readonly ILog _log;
        private readonly IBlockchainApiClientProvider _apiClientProvider;

        public StartAddressObservationCommandHandler(
            ILog log,
            IBlockchainApiClientProvider apiClientProvider)
        {
            _log = log;
            _apiClientProvider = apiClientProvider;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(StartAddressObservationCommand command, IEventPublisher publisher)
        {
#if DEBUG
            _log.WriteInfo(nameof(MonitoringTransactionHistoryCommand), command, "");
#endif
            try
            {
                var client = _apiClientProvider.Get(command.BlockchainType);
                if (client == null)
                {
                    _log.WriteInfo(nameof(MonitoringTransactionHistoryCommand), command, $"No client registered for blockchain of type {command.BlockchainType}");
                    return CommandHandlingResult.Fail(TimeSpan.FromMinutes(5));
                }

                if ((command.WalletAddressType & WalletAddressType.From) == WalletAddressType.From)
                {
                    await client.StartHistoryObservationOfOutgoingTransactionsAsync(command.WalletAddress);
                }

                if ((command.WalletAddressType & WalletAddressType.To) == WalletAddressType.To)
                {
                    await client.StartHistoryObservationOfIncomingTransactionsAsync(command.WalletAddress);
                }
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
