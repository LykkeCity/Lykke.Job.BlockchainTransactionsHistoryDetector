using Common.Log;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Contract;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Domain;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Workflow.Commands;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Workflow.Sagas
{
    public class WalletMonitorCreationSaga
    {
        private static readonly string Context = Lykke.Service.BlockchainWallets.Contract.BlockchainWalletsBoundedContext.Name;

        private readonly ILog _log;
        private readonly IWalletHistoryRepository _walletHistoryRepository;

        public WalletMonitorCreationSaga(ILog log, IWalletHistoryRepository walletHistoryRepository)
        {
            _log = log.CreateComponentScope(nameof(WalletMonitorCreationSaga));
            _walletHistoryRepository = walletHistoryRepository;
        }

        [UsedImplicitly]
        private async Task Handle(Lykke.Service.BlockchainWallets.Contract.Events.WalletCreatedEvent evt, ICommandSender sender)
        {
#if DEBUG
            _log.WriteInfo(nameof(Handle), evt, "");
#endif
            try
            {
                var old = await _walletHistoryRepository.TryGetAsync(evt.IntegrationLayerId, evt.Address, evt.AssetId);
                if (old != null)
                {
                    return;
                }
                var aggregate = await _walletHistoryRepository.GetOrAddAsync(
                    evt.IntegrationLayerId,
                    evt.Address,
                    () => WalletHistoryAggregate.CreateNew(evt.IntegrationLayerId, evt.Address, evt.AssetId, WalletAddressType.To));

                ChaosKitty.Meow(aggregate);

                if (aggregate.WalletHistoryState == WalletHistoryState.Started)
                {
                    sender.SendCommand(new StartAddressObservationCommand
                    {
                        BlockchainType = aggregate.BlockchainType,
                        WalletAddress = aggregate.WalletAddress,
                        WalletAddressType = aggregate.WalletAddressType
                    }, BoundedContext.BlockChainTransactionHistoryDetectorContext);
                }
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(WalletMonitorCreationSaga), evt, ex);
                throw;
            }
        }

        [UsedImplicitly]
        private async Task Handle(Lykke.Service.BlockchainWallets.Contract.Events.WalletDeletedEvent evt, ICommandSender sender)
        {
#if DEBUG
            _log.WriteInfo(nameof(Handle), evt, "");
#endif
            try
            {
                var aggregate = await _walletHistoryRepository.TryGetAsync(evt.IntegrationLayerId, evt.Address, evt.Address);

                if (aggregate == null)
                {
                    return;
                }

                ChaosKitty.Meow(aggregate);

                await _walletHistoryRepository.SaveAsync(WalletHistoryAggregate.StopObservation(
                    aggregate.BlockchainType,
                    aggregate.WalletAddress,
                    aggregate.AssetId,
                    aggregate.WalletAddressType));
            }
            catch (Exception ex)
            {
                _log.WriteError(nameof(WalletMonitorCreationSaga), evt, ex);
                throw;
            }
        }
    }
}
