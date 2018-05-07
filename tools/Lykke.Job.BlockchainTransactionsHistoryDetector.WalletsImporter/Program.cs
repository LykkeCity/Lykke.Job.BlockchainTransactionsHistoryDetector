using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Microsoft.Extensions.CommandLineUtils;

using TargetRepository = Lykke.Job.BlockchainTransactionsHistoryDetector.AzureRepositories.ObservableWalletsRepository;


namespace Lykke.Job.BlockchainTransactionsHistoryDetector.WalletsImporter
{
    internal static class Program
    {
        private const string SourceConnectionString = "sourceConnectionString";
        private const string TargetConnectionString = "targetConnectionString";


        private static void Main(string[] args)
        {
            var application = new CommandLineApplication
            {
                Description = "Regiters existing wallets as observable in the blockchain transactions history detector."
            };

            var arguments = new Dictionary<string, CommandArgument>
            {
                {
                    SourceConnectionString,
                    application.Argument
                    (
                        SourceConnectionString,
                        "Data connection string of a BlockchainWallets service."
                    )
                },
                {
                    TargetConnectionString,
                    application.Argument
                    (
                        TargetConnectionString,
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
                            arguments[SourceConnectionString].Value,
                            arguments[TargetConnectionString].Value
                        );
                    }

                    return 0;
                }
                catch (Exception e)
                {
                    Console.WriteLine();
                    Console.ForegroundColor = ConsoleColor.Red;
                    Console.WriteLine(e);

                    return 1;
                }
            });

            application.Execute(args);
        }

        private static async Task RegisterWalletsAsync(string sourceConnectionString, string targetConnectionString)
        {
            if (string.IsNullOrWhiteSpace(sourceConnectionString))
            {
                Console.WriteLine($"{sourceConnectionString} should be provided");

                return;
            }

            if (string.IsNullOrWhiteSpace(targetConnectionString))
            {
                Console.WriteLine($"{targetConnectionString} should be provided");

                return;
            }

            var log = new LogToConsole();

            var sourceRepository = SourceRepository.Create
            (
                new SettingsManager(targetConnectionString),
                log
            );
            
            var targetRepository = TargetRepository.Create
            (
                new SettingsManager(targetConnectionString),
                log
            );

            var sourceWallets = await sourceRepository.GetAllAsync();

            foreach (var sourceWallet in sourceWallets)
            {
                await targetRepository.AddIfNotExistsAsync
                (
                    blockchainType: sourceWallet.IntegrationLayerId,
                    walletAddress: sourceWallet.Address,
                    walletAssedId: sourceWallet.AssetId
                );
            }
        }
    }
}
