using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Repositories;
using Lykke.Service.BlockchainApi.Client;
using Lykke.SettingsReader;
using Microsoft.Extensions.CommandLineUtils;
using Microsoft.Extensions.Configuration;
using TargetRepository = Lykke.Job.BlockchainTransactionsHistoryDetector.AzureRepositories.ObservableWalletsRepository;


namespace Lykke.Job.BlockchainTransactionsHistoryDetector.WalletsImporter
{
    internal static class Program
    {
        private const string SourceSettingsUrl = "sourceSettingsUrl";
        private const string TargetSettingsUrl = "targetSettingsUrl";
        
        private static readonly ILog Log = new EmptyLog(); 

        
        private static void Main(string[] args)
        {
            var application = new CommandLineApplication
            {
                Description = "Regiters existing wallets as observable in the blockchain transactions history detector."
            };

            var arguments = new Dictionary<string, CommandArgument>
            {
                {
                    SourceSettingsUrl,
                    application.Argument
                    (
                        SourceSettingsUrl,
                        "Data connection string of a BlockchainWallets service."
                    )
                },
                {
                    TargetSettingsUrl,
                    application.Argument
                    (
                        TargetSettingsUrl,
                        "Data connection string of a BlockchainTransactionsHistoryDetector job."
                    )
                }
            };

            application.HelpOption("-? | -h | --help");
            application.OnExecute(async () =>
            {
                try
                {
                    if (arguments.Any(x => string.IsNullOrEmpty(x.Value.Value)))
                    {
                        application.ShowHelp();
                    }
                    else
                    {
                        await RegisterWalletsAsync
                        (
                            arguments[SourceSettingsUrl].Value,
                            arguments[TargetSettingsUrl].Value
                        );
                    }

                    return 0;
                }
                catch (Exception e)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e);
                    Console.ResetColor();

                    return 1;
                }
            });

            application.Execute(args);
        }

        private static async Task RegisterWalletsAsync(string sourceSettingsUrl, string targetSettingsUrl)
        {
            if (string.IsNullOrWhiteSpace(sourceSettingsUrl))
            {
                Console.WriteLine($"{sourceSettingsUrl} should be provided");

                return;
            }

            if (string.IsNullOrWhiteSpace(targetSettingsUrl))
            {
                Console.WriteLine($"{targetSettingsUrl} should be provided");

                return;
            }

            var (sourceSettings, targetSettings) = GetSettings(sourceSettingsUrl, targetSettingsUrl);
            var (sourceRepository, targetRepository) = GetRepositories(sourceSettings, targetSettings);
            var clients = await GetClientsAsync(sourceSettings.CurrentValue);

            await ImportWalletsAsync(sourceRepository, targetRepository, clients);
        }

        private static async Task<IDictionary<string, BlockchainApiClient>> GetClientsAsync(SourceSettings sourceSettings)
        {
            Console.WriteLine("Gettting blockchains, that support incoming transactions history");
            
            var clients = new Dictionary<string, BlockchainApiClient>();

            foreach (var blockchain in sourceSettings.BlockchainsIntegration.Blockchains)
            {
                var client = await GetClientIfTransactionHistoryIsSupportedAsync(blockchain);

                if (client != null)
                {
                    clients[blockchain.Type] = client;
                }
            }

            return clients;
        }
        
        private static async Task<BlockchainApiClient> GetClientIfTransactionHistoryIsSupportedAsync(BlockchainSettings blockchainSettings)
        {
            var client = new BlockchainApiClient(Log, blockchainSettings.ApiUrl);
            
            try
            {
                await client.StopHistoryObservationOfIncomingTransactionsAsync("fake-address");
            }
            catch (Exception e) when (e.InnerException is Refit.ApiException apiException)
            {
                // ReSharper disable once SwitchStatementMissingSomeCases
                switch (apiException.StatusCode)
                {
                        case HttpStatusCode.NotFound:
                        case HttpStatusCode.NotImplemented:
                            return null;
                        case HttpStatusCode.BadRequest:
                            break;
                        default:
                            throw;
                }
            }
            
            return client;
        }

        private static (SourceRepository, IObservableWalletsRepository) GetRepositories(IReloadingManager<SourceSettings> sourceSettings, IReloadingManager<TargetSettings> targetSettings)
        {
            var sourceConnectionString = sourceSettings
                .Nested(x => x.BlockchainWalletsService.Db.DataConnString);
            
            var sourceRepository = SourceRepository.Create(sourceConnectionString, Log);
            
            var targetConnectionString = targetSettings
                .Nested(x => x.BlockchainTransactionsHistoryDetectorJob.Db.DataConnString);
            
            var targetRepository = TargetRepository.Create(targetConnectionString, Log);

            return (sourceRepository, targetRepository);
        }
        
        private static (IReloadingManager<SourceSettings>, IReloadingManager<TargetSettings>) GetSettings(string sourceSettingsUrl, string targetSettingsUrl)
        {
            Console.WriteLine("Loading source configuration");
            
            var configurationBuilder = new ConfigurationBuilder();

            configurationBuilder.AddInMemoryCollection(new Dictionary<string, string>
            {
                {"SourceSettingsUrl", sourceSettingsUrl},
                {"TargetSettingsUrl", targetSettingsUrl}
            });
            
            var configuration = configurationBuilder.Build();
            
            var sourceSettings = configuration
                .LoadSettings<SourceSettings>("SourceSettingsUrl");
            
            var targetSettings = configuration
                .LoadSettings<TargetSettings>("TargetSettingsUrl");

            return (sourceSettings, targetSettings);
        }
        
        private static async Task ImportWalletAsync(IObservableWalletsRepository targetRepository, BlockchainApiClient client, SourceWalletEntity sourceWallet)
        {
            try
            {
                await client.StartHistoryObservationOfIncomingTransactionsAsync(sourceWallet.Address);
                    
                await targetRepository.AddIfNotExistsAsync
                (
                    blockchainType: sourceWallet.IntegrationLayerId,
                    walletAddress: sourceWallet.Address,
                    walletAssedId: sourceWallet.AssetId
                );
                    
                Console.WriteLine($"Wallet ({sourceWallet.IntegrationLayerId} {sourceWallet.Address}) imported");
            }
            catch (Exception e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine($"Failed to import wallet ({sourceWallet.IntegrationLayerId} {sourceWallet.Address}): {e.Message}");
                Console.ResetColor();
            }   
        }
        
        private static async Task ImportWalletsAsync(SourceRepository sourceRepository, IObservableWalletsRepository targetRepository, IDictionary<string, BlockchainApiClient> clients)
        {
            Console.WriteLine("Importing wallets");

            var sourceWallets = await sourceRepository.GetAllAsync();
            
            foreach (var sourceWallet in sourceWallets.Where(x => clients.ContainsKey(x.IntegrationLayerId)))
            {
                var client = clients[sourceWallet.IntegrationLayerId];

                await ImportWalletAsync(targetRepository, client, sourceWallet);
            }
        }
    }
}
