using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Domain;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Services.BLockchains;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Workflow.Commands;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Workflow.Events;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Workflow.CommandHandlers
{
    [UsedImplicitly]
    public class MonitoringTransactionHistoryCommandHandler
    {
        private int _batchSize = 100;
        private readonly ILog _log;
        private readonly ILastTransactionRepository _lastTransactionRepository;
        private readonly IBlockchainApiClientProvider _apiClientProvider;

        //private readonly IHotWalletsProvider _hotWalletsProvider;

        public MonitoringTransactionHistoryCommandHandler(
            ILog log, ILastTransactionRepository lastTransactionRepository, IBlockchainApiClientProvider apiClientProvider)
        {
            _log = log;
            _lastTransactionRepository = lastTransactionRepository;
            _apiClientProvider = apiClientProvider;
        }

        [UsedImplicitly]
        public async Task<CommandHandlingResult> Handle(MonitoringTransactionHistoryCommand command, IEventPublisher publisher)
        {
#if DEBUG
            _log.WriteInfo(nameof(MonitoringTransactionHistoryCommand), command, "");
#endif
            try
            {
                var client = _apiClientProvider.Get(command.BlockchainType);
                var lastTransaction = await _lastTransactionRepository.TryGetAsync(command.BlockchainType, command.WalletAddress);

                if ((command.WalletAddressType & WalletAddressType.From) == WalletAddressType.From)
                {
                    await = client.GetHistoryOfOutgoingTransactionsAsync();
                }
                client.
            }
            catch (Exception e)
            {

            }

            return Task.FromResult(CommandHandlingResult.Ok());
        }
    }
}
