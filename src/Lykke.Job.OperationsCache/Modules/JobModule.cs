using Autofac;
using AzureStorage;
using AzureStorage.Tables;
using Common.Log;
using Lykke.Job.OperationsCache.Core.Services;
using Lykke.Job.OperationsCache.Core.Settings;
using Lykke.Job.OperationsCache.Services;
using Lykke.SettingsReader;
using Lykke.Job.OperationsCache.PeriodicalHandlers;
using Lykke.Service.OperationsHistory.Services.InMemoryCache;
using Lykke.Service.OperationsRepository.Client;

namespace Lykke.Job.OperationsCache.Modules
{
    public class JobModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly ILog _log;

        public JobModule(IReloadingManager<AppSettings> settings, ILog log)
        {
            _log = log;
            _settings = settings;
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

            builder.RegisterInstance(
                AzureTableStorage<ClientSessionEntity>.Create(
                    _settings.ConnectionString(x => x.SessionSettings.Sessions.ConnectionString),
                    _settings.CurrentValue.SessionSettings.Sessions.TableName,
                    _log))
                .As<INoSQLTableStorage<ClientSessionEntity>>().SingleInstance();
            builder.RegisterType<ClientSessionsRepository>()
                .AsSelf()
                .SingleInstance();
            builder.RegisterType<OperationsHistoryReader>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterOperationsRepositoryClients(_settings.CurrentValue.OperationsRepositoryClient.ServiceUrl,
                _log, _settings.CurrentValue.OperationsRepositoryClient.RequestTimeout);
        }

        private void RegisterPeriodicalHandlers(ContainerBuilder builder)
        {
            builder.RegisterType<MyPeriodicalHandler>()
                .WithParameter(TypedParameter.From(_settings.CurrentValue.OperationsCacheJob.ExpirationPeriod))
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();
        }

    }
}
