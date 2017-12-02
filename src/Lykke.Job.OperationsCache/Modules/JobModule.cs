using Autofac;
using Common.Log;
using Lykke.Job.OperationsCache.Core.Services;
using Lykke.Job.OperationsCache.Core.Settings.JobSettings;
using Lykke.Job.OperationsCache.Services;
using Lykke.SettingsReader;
using Lykke.Job.OperationsCache.PeriodicalHandlers;
using Lykke.Service.OperationsHistory.Services.InMemoryCache;

namespace Lykke.Job.OperationsCache.Modules
{
    public class JobModule : Module
    {
        private readonly OperationsCacheSettings _settings;
        private readonly IReloadingManager<DbSettings> _dbSettingsManager;
        private readonly ILog _log;

        public JobModule(OperationsCacheSettings settings, IReloadingManager<DbSettings> dbSettingsManager, ILog log)
        {
            _settings = settings;
            _log = log;
            _dbSettingsManager = dbSettingsManager;
        }

        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterInstance(_log)
                .As<ILog>()
                .SingleInstance();

            builder.RegisterType<HealthService>()
                .As<IHealthService>()
                .SingleInstance();

            builder.RegisterType<StartupManager>()
                .As<IStartupManager>();

            builder.RegisterType<ShutdownManager>()
                .As<IShutdownManager>();
            RegisterPeriodicalHandlers(builder);


            builder.RegisterType<InMemoryCache>()
                .As<IHistoryCache>()
                .SingleInstance();
        }

        private void RegisterPeriodicalHandlers(ContainerBuilder builder)
        {
            builder.RegisterType<MyPeriodicalHandler>()
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();
        }

    }
}
