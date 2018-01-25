using System.Collections.Generic;
using Autofac;
using Common.Log;
using Inceptum.Cqrs.Configuration;
using Inceptum.Messaging;
using Inceptum.Messaging.Contract;
using Inceptum.Messaging.RabbitMq;
using Lykke.Cqrs;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Contract;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Settings.JobSettings;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Workflow.CommandHandlers;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Workflow.Commands;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Workflow.Sagas;
using Lykke.Job.BlockchainOperationsExecutor.Contract;
using Lykke.Messaging;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Modules
{
    public class CqrsModule : Module
    {
        private readonly CqrsSettings _settings;
        private readonly ChaosSettings _chaosSettings;
        private readonly ILog _log;

        public CqrsModule(CqrsSettings settings, ChaosSettings chaosSettings, ILog log)
        {
            _settings = settings;
            _chaosSettings = chaosSettings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            if (_chaosSettings != null)
            {
                ChaosKitty.StateOfChaos = _chaosSettings.StateOfChaos;
            }

            builder.Register(context => new AutofacDependencyResolver(context)).As<IDependencyResolver>().SingleInstance();

            var rabbitMqSettings = new RabbitMQ.Client.ConnectionFactory
            {
                Uri = _settings.RabbitConnectionString
            };
            var messagingEngine = new MessagingEngine(_log,
                new TransportResolver(new Dictionary<string, TransportInfo>
                {
                    {
                        "RabbitMq",
                        new TransportInfo(rabbitMqSettings.Endpoint.ToString(), rabbitMqSettings.UserName,
                            rabbitMqSettings.Password, "None", "RabbitMq")
                    }
                }),
                new RabbitMqTransportFactory());

            // Sagas
            builder.RegisterType<WalletMonitorCreationSaga>();

            // Command handlers
            builder.RegisterType<StartAddressObservationCommandHandler>();
            builder.RegisterType<MonitoringTransactionHistoryCommandHandler>();

            builder.Register(ctx => CreateEngine(ctx, messagingEngine))
                .As<ICqrsEngine>()
                .SingleInstance()
                .AutoActivate();
        }

        private CqrsEngine CreateEngine(IComponentContext ctx, IMessagingEngine messagingEngine)
        {
            var defaultRetryDelay = (long)_settings.RetryDelay.TotalMilliseconds;

            const string defaultPipeline = "commands";
            const string defaultRoute = "self";

            return new CqrsEngine(
                _log,
                ctx.Resolve<IDependencyResolver>(),
                messagingEngine,
                new DefaultEndpointProvider(),
                true,
                Register.DefaultEndpointResolver(new RabbitMqConventionEndpointResolver(
                    "RabbitMq",
                    "messagepack", 
                    environment: "lykke")),

                Register.BoundedContext(BoundedContext.BlockChainTransactionHistoryDetectorContext)
                    .FailedCommandRetryDelay(defaultRetryDelay)
                    .ListeningCommands(typeof(MonitoringTransactionHistoryCommand))
                    .On(defaultRoute)
                    .WithLoopback()
                    .WithCommandsHandler<MonitoringTransactionHistoryCommandHandler>()
                    .ListeningCommands(typeof(StartAddressObservationCommand))
                    .On(defaultRoute)
                    .WithCommandsHandler<StartAddressObservationCommandHandler>()
                    .ProcessingOptions(defaultRoute).MultiThreaded(8).QueueCapacity(1024),

                Register.Saga<WalletMonitorCreationSaga>($"{BoundedContext.BlockChainTransactionHistoryDetectorContext}.saga")
                    .ListeningEvents(
                        typeof(Lykke.Service.BlockchainWallets.Contract.Events.WalletCreatedEvent),
                        typeof(Lykke.Service.BlockchainWallets.Contract.Events.WalletDeletedEvent))
                    .From(Lykke.Service.BlockchainWallets.Contract.BlockchainWalletsBoundedContext.Name)
                    .On(defaultRoute)
                    .PublishingCommands(typeof(StartAddressObservationCommand))
                    .To(BoundedContext.BlockChainTransactionHistoryDetectorContext)
                    .With(defaultPipeline));
        }
    }
}
