﻿using System.Collections.Generic;
using System.Linq;
using Autofac;
using Common.Log;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Services.BLockchains;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Services.Blockchains;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Settings;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Settings.Blockchain;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Settings.JobSettings;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Workflow.PeriodicalHandlers;
using Lykke.Service.BlockchainApi.Client;
using Lykke.Service.BlockchainWallets.Client;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Modules
{
    public class BlockchainsModule : Module
    {
        private readonly BlockchainTransactionsHistoryDetectorSettings _settings;
        private readonly BlockchainsIntegrationSettings _blockchainsIntegrationSettings;
        private readonly ILog _log;

        public BlockchainsModule(
            BlockchainTransactionsHistoryDetectorSettings settings,
            BlockchainsIntegrationSettings blockchainsIntegrationSettings,
            ILog log)
        {
            _settings = settings;
            _blockchainsIntegrationSettings = blockchainsIntegrationSettings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<HotWalletsProvider>()
                .As<IHotWalletsProvider>()
                .WithParameter(TypedParameter.From<IReadOnlyDictionary<string, string>>(_blockchainsIntegrationSettings.Blockchains.ToDictionary(b => b.Type, b => b.HotWalletAddress)))
                .SingleInstance();

            builder.RegisterType<BlockchainApiClientProvider>()
                .As<IBlockchainApiClientProvider>();

            foreach (var blockchain in _blockchainsIntegrationSettings.Blockchains.Where(b => !b.IsDisabled))
            {
                _log.WriteInfo("Blockchains registration", "", 
                    $"Registering blockchain: {blockchain.Type} -> \r\nAPI: {blockchain.ApiUrl}\r\nHW: {blockchain.HotWalletAddress}");

                builder.RegisterType<BlockchainApiClient>()
                    .Named<IBlockchainApiClient>(blockchain.Type)
                    .WithParameter(TypedParameter.From(blockchain.ApiUrl))
                    .SingleInstance();


                //builder.RegisterType<TransactionHistoryProcessingPeriodicalHandler>()
                //    .As<IStartable>()
                //    .AutoActivate()
                //    .SingleInstance()
                //    .WithParameter(TypedParameter.From(_settings.Monitoring.Period))
                //    .WithParameter(TypedParameter.From(_settings.Requests.BatchSize))
                //    .WithParameter(TypedParameter.From(blockchain.Type));
            }
        }
    }
}
