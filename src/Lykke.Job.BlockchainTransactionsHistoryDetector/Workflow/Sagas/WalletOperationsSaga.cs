using System;
using System.Threading.Tasks;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Cqrs;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Repositories;
using Lykke.Service.BlockchainWallets.Contract.Events;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Workflow.Sagas
{
    public class WalletOperationsSaga
    {
        private readonly IObservableWalletsRepository _observableWalletsRepository;
        private readonly ILog _log;


        public WalletOperationsSaga(
            IObservableWalletsRepository observableWalletsRepository,
            ILog log)
        {
            _observableWalletsRepository = observableWalletsRepository;
            _log = log;
        }


        [UsedImplicitly]
        private async Task Handle(WalletCreatedEvent evt, ICommandSender sender)
        {
            _log.WriteInfo(nameof(WalletCreatedEvent), evt, "");

            try
            {
                await _observableWalletsRepository.AddIfNotExistsAsync
                (
                    blockchainType: evt.IntegrationLayerId, 
                    walletAddress: evt.Address
                );
            }
            catch (Exception e)
            {
                _log.WriteError(nameof(WalletCreatedEvent), evt, e);

                throw;
            }
        }

        [UsedImplicitly]
        private async Task Handle(WalletDeletedEvent evt, ICommandSender sender)
        {
            _log.WriteInfo(nameof(WalletDeletedEvent), evt, "");

            try
            {
                await _observableWalletsRepository.DeleteIfExistsAsync
                (
                    blockchainType: evt.IntegrationLayerId,
                    walletAddress: evt.Address
                );
            }
            catch (Exception e)
            {
                _log.WriteError(nameof(WalletDeletedEvent), evt, e);

                throw;
            }
        }
    }
}
