using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureStorage.Tables;
using Common.Log;
using JetBrains.Annotations;
using Lykke.Common.ApiLibrary.Middleware;
using Lykke.Common.ApiLibrary.Swagger;
using Lykke.Common.Api.Contract.Responses;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Settings;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Modules;
using Lykke.Logs;
using Lykke.Logs.Slack;
using Lykke.SettingsReader;
using Lykke.SlackNotification.AzureQueue;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;


namespace Lykke.Job.BlockchainTransactionsHistoryDetector
{
    [UsedImplicitly]
    public class Startup
    {
        private readonly IConfigurationRoot _configuration;

        private IContainer _applicationContainer;
        private ILog _log;


        public Startup(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddEnvironmentVariables();

            _configuration = builder.Build();
        }

        [UsedImplicitly]
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {
            try
            {
                services.AddMvc()
                    .AddJsonOptions(options =>
                    {
                        options.SerializerSettings.ContractResolver =
                            new Newtonsoft.Json.Serialization.DefaultContractResolver();
                    });

                services.AddSwaggerGen(options =>
                {
                    options.DefaultLykkeConfiguration("v1", "BlockchainTransactionsHistoryDetector API");
                });

                var builder = new ContainerBuilder();
                var appSettings = _configuration.LoadSettings<AppSettings>();

                _log = CreateLogWithSlack(services, appSettings);

                builder.RegisterModule
                (
                    new JobModule
                    (
                        _log
                    )
                );

                builder.RegisterModule
                (
                    new RepositoriesModule
                    (
                        appSettings.Nested(x => x.BlockchainTransactionsHistoryDetectorJob.Db),
                        _log
                    )
                );

                builder.RegisterModule
                (
                    new CqrsModule
                    (
                        appSettings.CurrentValue.BlockchainsIntegration,
                        appSettings.CurrentValue.BlockchainTransactionsHistoryDetectorJob.Cqrs,
                        _log
                    )
                );

                builder.Populate(services);

                _applicationContainer = builder.Build();

                return new AutofacServiceProvider(_applicationContainer);
            }
            catch (Exception ex)
            {
                _log?.WriteFatalError(nameof(Startup), nameof(ConfigureServices), ex);

                throw;
            }
        }

        [UsedImplicitly]
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, IApplicationLifetime appLifetime)
        {
            try
            {
                if (env.IsDevelopment())
                {
                    app.UseDeveloperExceptionPage();
                }

                app.UseLykkeForwardedHeaders();
                app.UseLykkeMiddleware("BlockchainTransactionsHistoryDetector", ex => new ErrorResponse {ErrorMessage = "Technical problem"});

                app.UseMvc();
                app.UseSwagger(c =>
                {
                    c.PreSerializeFilters.Add((swagger, httpReq) => swagger.Host = httpReq.Host.Value);
                });
                app.UseSwaggerUI(x =>
                {
                    x.RoutePrefix = "swagger/ui";
                    x.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                });
                app.UseStaticFiles();

                appLifetime.ApplicationStarted.Register(() => StartApplication().GetAwaiter().GetResult());
                appLifetime.ApplicationStopped.Register(() => CleanUp().GetAwaiter().GetResult());
            }
            catch (Exception ex)
            {
                _log?.WriteFatalError(nameof(Startup), nameof(Configure), ex);

                throw;
            }
        }

        private async Task StartApplication()
        {
            try
            {
                await _log.WriteMonitorAsync("", Program.EnvInfo, "Started");
            }
            catch (Exception ex)
            {
                await _log.WriteFatalErrorAsync(nameof(Startup), nameof(StartApplication), "", ex);

                throw;
            }
        }
        
        private async Task CleanUp()
        {
            try
            {
                if (_log != null)
                {
                    await _log.WriteMonitorAsync("", Program.EnvInfo, "Terminating");
                }
                
                _applicationContainer.Dispose();
            }
            catch (Exception ex)
            {
                if (_log != null)
                {
                    await _log.WriteFatalErrorAsync(nameof(Startup), nameof(CleanUp), "", ex);
                    (_log as IDisposable)?.Dispose();
                }

                throw;
            }
        }

        private static ILog CreateLogWithSlack(IServiceCollection services, IReloadingManager<AppSettings> settings)
        {
            var consoleLogger = new LogToConsole();
            var aggregateLogger = new AggregateLogger();

            aggregateLogger.AddLog(consoleLogger);

            var dbLogConnectionStringManager = settings.Nested(x => x.BlockchainTransactionsHistoryDetectorJob.Db.LogsConnString);
            var dbLogConnectionString = dbLogConnectionStringManager.CurrentValue;

            if (string.IsNullOrEmpty(dbLogConnectionString))
            {
                consoleLogger.WriteWarningAsync(nameof(Startup), nameof(CreateLogWithSlack), "Table loggger is not inited").Wait();

                return aggregateLogger;
            }

            if (dbLogConnectionString.StartsWith("${") && dbLogConnectionString.EndsWith("}"))
            {
                throw new InvalidOperationException($"LogsConnString {dbLogConnectionString} is not filled in settings");
            }

            var persistenceManager = new LykkeLogToAzureStoragePersistenceManager
            (
                AzureTableStorage<LogEntity>.Create(dbLogConnectionStringManager, "BlockchainTransactionsHistoryDetectorLog", consoleLogger),
                consoleLogger
            );

            // Creating slack notification service, which logs own azure queue processing messages to aggregate log
            var slackService = services.UseSlackNotificationsSenderViaAzureQueue(new AzureQueueIntegration.AzureQueueSettings
            {
                ConnectionString = settings.CurrentValue.SlackNotifications.AzureQueue.ConnectionString,
                QueueName = settings.CurrentValue.SlackNotifications.AzureQueue.QueueName
            }, aggregateLogger);

            var slackNotificationsManager = new LykkeLogToAzureSlackNotificationsManager(slackService, consoleLogger);

            // Creating azure storage logger, which logs own messages to concole log
            var azureStorageLogger = new LykkeLogToAzureStorage
            (
                persistenceManager,
                slackNotificationsManager,
                consoleLogger
            );

            azureStorageLogger.Start();

            aggregateLogger.AddLog(azureStorageLogger);

            var allMessagesSlackLogger = LykkeLogToSlack.Create
            (
                slackService,
                "BlockChainIntegration",
                // ReSharper disable once RedundantArgumentDefaultValue
                LogLevel.All
            );

            aggregateLogger.AddLog(allMessagesSlackLogger);

            var importantMessagesSlackLogger = LykkeLogToSlack.Create
            (
                slackService,
                "BlockChainIntegrationImportantMessages",
                LogLevel.All ^ LogLevel.Info
            );

            aggregateLogger.AddLog(importantMessagesSlackLogger);

            return aggregateLogger;
        }
    }
}
