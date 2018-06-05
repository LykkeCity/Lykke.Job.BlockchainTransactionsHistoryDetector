using Autofac;
using Common.Log;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Core.Services;
using Lykke.Job.BlockchainTransactionsHistoryDetector.Services;


namespace Lykke.Job.BlockchainTransactionsHistoryDetector.Modules
{
    public class JobModule : Module
    {
        private readonly ILog _log;


        public JobModule(ILog log)
        {
            _log = log;
        }


        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();
        }
    }
}
