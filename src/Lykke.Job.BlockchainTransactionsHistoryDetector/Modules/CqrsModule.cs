using System.Collections.Generic;
using Autofac;
using Common.Log;
using Lykke.Cqrs;
using Lykke.Cqrs.Configuration;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Contract;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Contract.Events;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Settings.BlockchainsIntegration;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Settings.Job;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Workflow.Processes;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Workflow.Sagas;
using Lykke.Messaging;
using Lykke.Messaging.Contract;
using Lykke.Messaging.RabbitMq;
using Lykke.Service.BlockchainWallets.Contract;
using Lykke.Service.BlockchainWallets.Contract.Events;
using RabbitMQ.Client;

namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Modules
{
    public class CqrsModule : Module
    {
        private readonly BlockchainsIntegrationSettings _blockchainsIntegrationSettings;
        private readonly CqrsSettings _cqrsSettings;
        private readonly ILog _log;

        public CqrsModule(
            BlockchainsIntegrationSettings blockchainsIntegrationSettings,
            CqrsSettings cqrsSettings,
            ILog log)
        {
            _blockchainsIntegrationSettings = blockchainsIntegrationSettings;
            _cqrsSettings = cqrsSettings;
            _log = log;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder
                .Register(ctx => new AutofacDependencyResolver(ctx))
                .As<IDependencyResolver>()
                .SingleInstance();

            builder
                .Register(ctx => _blockchainsIntegrationSettings)
                .AsSelf()
                .SingleInstance();

            var rabbitMqSettings = new ConnectionFactory
            {
                Uri = _cqrsSettings.RabbitConnectionString
            };

            var transportInfo = new TransportInfo
            (
                rabbitMqSettings.Endpoint.ToString(),
                rabbitMqSettings.UserName,
                rabbitMqSettings.Password,
                "None",
                "RabbitMq"
            );

            var transports = new Dictionary<string, TransportInfo>
            {
                {"RabbitMq", transportInfo}
            };

            var messagingEngine = new MessagingEngine
            (
                _log,
                new TransportResolver(transports),
                new RabbitMqTransportFactory()
            );

            builder
                .Register(ctx => CreateEngine(ctx, messagingEngine))
                .As<ICqrsEngine>()
                .SingleInstance()
                .AutoActivate();

            builder
                .RegisterType<NewTransactionDetectionProcess>()
                .AsSelf()
                .SingleInstance();

            builder
                .RegisterType<WalletOperationsSaga>()
                .AsSelf()
                .SingleInstance();
        }

        private CqrsEngine CreateEngine(IComponentContext ctx, IMessagingEngine messagingEngine)
        {
            var endpointResolver = new RabbitMqConventionEndpointResolver
            (
                transport: "RabbitMq",
                serializationFormat: "messagepack",
                environment: "lykke"
            );

            var registrations = new IRegistration[]
            {
                Register
                    .DefaultEndpointResolver(endpointResolver),

                Register
                    .BoundedContext(BoundedContext.Name)
                    .WithProcess<NewTransactionDetectionProcess>()
                    .PublishingEvents(typeof(TransactionDetectedEvent))
                    .With(BoundedContext.EventsRoute),

                Register
                    .Saga<WalletOperationsSaga>("wallet-operations-saga")
                    .ListeningEvents(typeof(WalletCreatedEvent), typeof(WalletDeletedEvent))
                    .From(BlockchainWalletsBoundedContext.Name)
                    .On(BlockchainWalletsBoundedContext.EventsRoute)
            };
            
            return new CqrsEngine
            (
                _log,
                ctx.Resolve<IDependencyResolver>(),
                messagingEngine,
                new DefaultEndpointProvider(),
                true,
                registrations
            );
        }
    }
}
