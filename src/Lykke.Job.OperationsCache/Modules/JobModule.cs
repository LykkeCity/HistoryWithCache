using Autofac;
using AzureRepositories.CashOperations;
using AzureStorage;
using AzureStorage.Tables;
using AzureStorage.Tables.Templates.Index;
using Common.Log;
using Core.CashOperations;
using Lykke.Job.OperationsCache.Core.Services;
using Lykke.Job.OperationsCache.Services;
using Lykke.SettingsReader;
using Lykke.Job.OperationsCache.PeriodicalHandlers;
using Lykke.Job.OperationsCache.Services.InMemoryCache;
using Lykke.Job.OperationsCache.Services.OperationsHistory;
using Lykke.Service.OperationsRepository.AzureRepositories.CashOperations;
using Lykke.Service.OperationsRepository.Core.CashOperations;
using Core.BitCoin;
using AzureRepositories.Bitcoin;
using Core.Exchange;
using AzureRepositories.Exchange;
using Lykke.Service.Assets.Client;
using System;
using Core;
using System.Linq;
using Common;
using Lykke.Job.OperationsCache.Handlers;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.Session.Client;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;
using AppSettings = Lykke.Job.OperationsCache.Settings.AppSettings;

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

            builder.RegisterInstance(_settings.CurrentValue.OperationsCacheJob);
            builder.RegisterInstance(_settings.CurrentValue.RabbitMq);

            if (_settings.CurrentValue.OperationsCacheJob.InMemory)
            {
                builder.RegisterType<InMemoryStorage>()
                    .As<IStorage>()
                    .SingleInstance();
            }
            else
            {
                builder.RegisterType<RedisStorage>()
                    .As<IStorage>()
                    .SingleInstance();
            }

            builder.RegisterType<HistoryCache>()
                .WithParameter("valuesPerPage", _settings.CurrentValue.OperationsCacheJob.ItemsPerPage)
                .WithParameter("maxHistoryLengthPerClient", _settings.CurrentValue.OperationsCacheJob.MaxHistoryLengthPerClient)
                .WithParameter("saveHistoryLengthPerClient", _settings.CurrentValue.OperationsCacheJob.SaveHistoryLengthPerClient)
                .As<IHistoryCache>()
                .SingleInstance();

            builder.RegisterType<ClientSessionsRepository>()
                .AsSelf()
                .SingleInstance();

            builder.RegisterType<OperationsHistoryRepoReader>()
                .As<IOperationsHistoryReader>()
                .SingleInstance();

            builder.RegisterInstance<IAssetsService>(
                new AssetsService(new Uri(_settings.CurrentValue.AssetsServiceClient.ServiceUrl)));
            
            builder.RegisterRedisClientSession(_settings.CurrentValue.SessionSettings);

            RegisterRepositories(builder);

            RegisterCachedDicts(builder);

            builder.RegisterType<DelayWarmUp>()
                .WithParameter("delayPeriod", _settings.CurrentValue.OperationsCacheJob.UpdateDelayPeriod)
                .WithParameter("excludeList", _settings.CurrentValue.OperationsCacheJob.ExcludeClientIdList)
                .As<IDelayWarmUp>()
                .OnRelease(x => x.Dispose());

            builder.RegisterType<TransferQueue>()
                .SingleInstance();

            builder.RegisterType<TradeQueue>()
                .SingleInstance();

            builder.RegisterType<CashInOutQueue>()
                .SingleInstance();

            builder.RegisterType<LimitTradeQueue>()
                .SingleInstance();
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
            builder.RegisterInstance<ICashOperationsRepository>(
                new CashOperationsRepository(
                    AzureTableStorage<CashInOutOperationEntity>.Create(_settings.ConnectionString(x => x.OperationsCacheJob.Db.CashOperationsConnString), "OperationsCash", _log),
                    AzureTableStorage<AzureIndex>.Create(_settings.ConnectionString(x => x.OperationsCacheJob.Db.CashOperationsConnString), "OperationsCash", _log)));

            builder.RegisterInstance<IClientTradesRepository>(
                new ClientTradesRepository(
                    AzureTableStorage<ClientTradeEntity>.Create(_settings.ConnectionString(x => x.OperationsCacheJob.Db.ClientTradesConnString), "Trades", _log)));

            builder.RegisterInstance<ITransferEventsRepository>(
                new TransferEventsRepository(
                    AzureTableStorage<TransferEventEntity>.Create(_settings.ConnectionString(x => x.OperationsCacheJob.Db.TransferConnString), "Transfers", _log),
                    AzureTableStorage<AzureIndex>.Create(_settings.ConnectionString(x => x.OperationsCacheJob.Db.TransferConnString), "Transfers", _log)));

            builder.RegisterInstance<ICashOutAttemptRepository>(
                new CashOutAttemptRepository(
                    AzureTableStorage<CashOutAttemptEntity>.Create(_settings.ConnectionString(x => x.OperationsCacheJob.Db.CashOutAttemptConnString), "CashOutAttempt", _log)));

            builder.RegisterInstance<ILimitTradeEventsRepository>(
                new LimitTradeEventsRepository(
                    AzureTableStorage<LimitTradeEventEntity>.Create(_settings.ConnectionString(x => x.OperationsCacheJob.Db.LimitTradesConnString), "LimitTradeEvents", _log)));

            builder.RegisterInstance<IWalletCredentialsRepository>(
                new WalletCredentialsRepository(
                    AzureTableStorage<WalletCredentialsEntity>.Create(_settings.ConnectionString(x => x.OperationsCacheJob.Db.ClientPersonalInfoConnString),
                        "WalletCredentials", _log)));

            builder.RegisterInstance<IMarketOrdersRepository>(
                new MarketOrdersRepository(AzureTableStorage<MarketOrderEntity>.Create(_settings.ConnectionString(x => x.OperationsCacheJob.Db.MarketOrdersConnString),
                    "MarketOrders", _log)));
        }

        private void RegisterCachedDicts(ContainerBuilder builder)
        {
            builder.Register(x =>
            {
                var assetsService = x.Resolve<IComponentContext>().Resolve<IAssetsService>();

                return new CachedAssetsDictionary
                (
                    async () => (await assetsService.AssetGetAllAsync(includeNonTradable: true)).ToDictionary(itm => itm.Id)
                );

            }).SingleInstance();

            builder.Register(x =>
            {
                var assetsService = x.Resolve<IComponentContext>().Resolve<IAssetsService>();

                return new CachedTradableAssetsDictionary
                (
                    async () => (await assetsService.AssetGetAllAsync(includeNonTradable: false)).ToDictionary(itm => itm.Id)
                );

            }).SingleInstance();

            builder.Register(x =>
            {

                var assetsService = x.Resolve<IComponentContext>().Resolve<IAssetsService>();

                return new CachedDataDictionary<string, AssetPair>
                (
                    async () => (await assetsService.AssetPairGetAllAsync()).ToDictionary(itm => itm.Id)
                );

            }).SingleInstance();

            builder.Register(x =>
            {
                var sessionsRepository = x.Resolve<IComponentContext>().Resolve<ClientSessionsRepository>();

                return new CachedSessionsDictionary
                (
                    async () => (await sessionsRepository.GetClientsIds()).Distinct().ToDictionary(itm => itm)
                );

            }).SingleInstance();
        }
    }
}
