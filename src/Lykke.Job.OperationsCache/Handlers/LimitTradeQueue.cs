using System;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.OperationsCache.Handlers.Models;
using Lykke.Job.OperationsCache.Services;
using Lykke.Job.OperationsCache.Settings.JobSettings;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;

namespace Lykke.Job.OperationsCache.Handlers
{
    public class LimitTradeQueue : IQueueSubscriber
    {
#if DEBUG
        private const string QueueName = "transactions.limit-trades-dev.cache";
#else
        private const string QueueName = "transactions.limit-trades.cache";
        private const bool QueueDurable = true;
#endif
                
        private readonly ILog _log;
        
        private readonly RabbitMqSettings _rabbitConfig;
        private readonly IDelayWarmUp _delayWampUpSubject;
        private RabbitMqSubscriber<LimitQueueItem> _subscriber;

        public LimitTradeQueue(
            RabbitMqSettings config,
            IDelayWarmUp delayWampUpSubject,
            ILog log)
        {
            _rabbitConfig = config;
            _delayWampUpSubject = delayWampUpSubject;
            _log = log;
        }

        public void Start()
        {
            var settings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = _rabbitConfig.ConnectionString,
                QueueName = QueueName,
                ExchangeName = _rabbitConfig.ExchangeLimit,
                DeadLetterExchangeName = $"{_rabbitConfig.ExchangeLimit}.dlx",
                RoutingKey = "",
                IsDurable = false
            };

            try
            {
                _subscriber = new RabbitMqSubscriber<LimitQueueItem>(settings, new DeadQueueErrorHandlingStrategy(_log, settings))
                    .SetMessageDeserializer(new JsonMessageDeserializer<LimitQueueItem>())
                    .SetMessageReadStrategy(new MessageReadQueueStrategy())
                    .Subscribe(ProcessMessage)
                    .CreateDefaultBinding()
                    .SetLogger(_log)
                    .Start();
            }
            catch (Exception ex)
            {
                _log.WriteErrorAsync(nameof(LimitTradeQueue), nameof(Start), null, ex).Wait();
                throw;
            }
        }

        public void Stop()
        {
            _subscriber?.Stop();
        }

        public async Task ProcessMessage(LimitQueueItem tradeItem)
        {
            foreach (var limitOrder in tradeItem.Orders)
            {
                await _delayWampUpSubject.OnNewOperation(limitOrder.Order.ClientId);
            }            
        }

        
        public void Dispose()
        {
            Stop();
        }
    }
}

