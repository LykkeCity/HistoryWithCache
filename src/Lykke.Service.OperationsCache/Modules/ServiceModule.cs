using System;
using System.Linq;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using AzureRepositories.CashOperations;
using AzureRepositories.Exchange;
using AzureStorage.Tables;
using AzureStorage.Tables.Templates.Index;
using Common;
using Common.Log;
using Core;
using Core.CashOperations;
using Core.Exchange;
using Core.Services;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.OperationsCache.Settings;
using Lykke.SettingsReader;
using Microsoft.Extensions.DependencyInjection;
using Services;
using Services.InMemoryCache;

namespace Lykke.Service.OperationsCache.Modules
{
    public class ServiceModule : Module
    {
        private readonly IReloadingManager<AppSettings> _settings;
        private readonly ILog _log;
        // NOTE: you can remove it if you don't need to use IServiceCollection extensions to register service specific dependencies
        private readonly IServiceCollection _services;

        public ServiceModule(IReloadingManager<AppSettings> settings, ILog log)
        {
            _settings = settings;
            _log = log;

            _services = new ServiceCollection();
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
            
            _services.AddDistributedRedisCache(options =>
            {
                options.Configuration = _settings.CurrentValue.RedisSettings.Configuration;
                options.InstanceName = _settings.CurrentValue.OperationsCacheJob.CacheInstanceName;
            });

            builder.RegisterType<RedisStorage>()
                .As<IStorage>()
                .SingleInstance();
            
            builder.RegisterInstance<IAssetsService>(
                new AssetsService(new Uri(_settings.CurrentValue.AssetsServiceClient.ServiceUrl)));
            
            RegisterRepositories(builder);
            RegisterCachedDicts(builder);
            
            builder.RegisterType<OperationsHistoryRepoReader>()
                .As<IOperationsHistoryReader>()
                .SingleInstance();

            builder.RegisterType<HistoryCache>()
                .WithParameter("valuesPerPage", _settings.CurrentValue.OperationsCacheJob.ItemsPerPage)
                .WithParameter("maxHistoryLengthPerClient", _settings.CurrentValue.OperationsCacheJob.MaxHistoryLengthPerClient)
                .WithParameter("saveHistoryLengthPerClient", _settings.CurrentValue.OperationsCacheJob.SaveHistoryLengthPerClient)
                .As<IHistoryCache>()
                .SingleInstance();

            builder.Populate(_services);
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

                return new CachedDataDictionary<string, AssetPair>
                (
                    async () => (await assetsService.AssetPairGetAllAsync()).ToDictionary(itm => itm.Id)
                );

            }).SingleInstance();
        }
    }
}
