using Autofac;
using AzureRepositories.CashOperations;
using AzureStorage;
using AzureStorage.Tables;
using AzureStorage.Tables.Templates.Index;
using Common.Log;
using Core.CashOperations;
using Lykke.Job.OperationsCache.Core.Services;
using Lykke.Job.OperationsCache.Core.Settings;
using Lykke.Job.OperationsCache.Services;
using Lykke.SettingsReader;
using Lykke.Job.OperationsCache.PeriodicalHandlers;
using Lykke.Job.OperationsCache.Services.InMemoryCache;
using Lykke.Job.OperationsCache.Services.OperationsHistory;
//using Lykke.Service.OperationsRepository.Client;
using Lykke.Service.OperationsRepository.AzureRepositories.CashOperations;
using Lykke.Service.OperationsRepository.Core.CashOperations;

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
                .WithParameter("valuesPerPage", _settings.CurrentValue.OperationsCacheJob.ItemsPerPage)
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

            builder.RegisterType<OperationsHistoryRepoReader>()
                .As<IOperationsHistoryReader>()
                .SingleInstance();

            RegisterRepositories(builder);
        }

        private void RegisterPeriodicalHandlers(ContainerBuilder builder)
        {
            builder.RegisterType<MyPeriodicalHandler>()
                .WithParameter("expirationPeriod", _settings.CurrentValue.OperationsCacheJob.ExpirationPeriod)
                .WithParameter("excludeList", _settings.CurrentValue.OperationsCacheJob.ExcludeClientIdList)
                .As<IStartable>()
                .AutoActivate()
                .SingleInstance();
        }

        private void RegisterRepositories(ContainerBuilder builder)
        {
            var connectionString = _settings.ConnectionString(x => x.OperationsRepositoryService.Db.RepoConnectionString);
            builder.RegisterInstance<ICashOperationsRepository>(
                new CashOperationsRepository(
                    AzureTableStorage<CashInOutOperationEntity>.Create(connectionString, "OperationsCash", _log),
                    AzureTableStorage<AzureIndex>.Create(connectionString, "OperationsCash", _log)));

            builder.RegisterInstance<IClientTradesRepository>(
                new ClientTradesRepository(
                    AzureTableStorage<ClientTradeEntity>.Create(connectionString, "Trades", _log)));

            builder.RegisterInstance<ITransferEventsRepository>(
                new TransferEventsRepository(
                    AzureTableStorage<TransferEventEntity>.Create(connectionString, "Transfers", _log),
                    AzureTableStorage<AzureIndex>.Create(connectionString, "Transfers", _log)));

            builder.RegisterInstance<ICashOutAttemptRepository>(
                new CashOutAttemptRepository(
                    AzureTableStorage<CashOutAttemptEntity>.Create(connectionString, "CashOutAttempt", _log)));

            builder.RegisterInstance<ILimitTradeEventsRepository>(
                new LimitTradeEventsRepository(
                    AzureTableStorage<LimitTradeEventEntity>.Create(connectionString, "LimitTradeEvents", _log)));
        }
    }
}
