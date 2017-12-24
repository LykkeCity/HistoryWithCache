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
    public class CashInOutQueue : IQueueSubscriber
    {
        private const string QueueName = "transactions.cashinout.cache";

        private readonly ILog _log;
        private readonly IDelayWarmUp _delayWampUpSubject;
        
        private readonly RabbitMqSettings _rabbitConfig;
        private RabbitMqSubscriber<CashInOutQueueMessage> _subscriber;

        public CashInOutQueue(
            RabbitMqSettings config, 
            ILog log,
            IDelayWarmUp delayWampUpSubject)
        {
            _rabbitConfig = config;
            _log = log;
            _delayWampUpSubject = delayWampUpSubject;            
        }

        public void Start()
        {
            var settings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = _rabbitConfig.ConnectionString,
                QueueName = QueueName,
                ExchangeName = _rabbitConfig.ExchangeCashOperation,
                DeadLetterExchangeName = $"{_rabbitConfig.ExchangeCashOperation}.dlx",
                RoutingKey = "",
                IsDurable = true
            };

            try
            {
                _subscriber = new RabbitMqSubscriber<CashInOutQueueMessage>(settings, new DeadQueueErrorHandlingStrategy(_log, settings))
                    .SetMessageDeserializer(new JsonMessageDeserializer<CashInOutQueueMessage>())
                    .SetMessageReadStrategy(new MessageReadQueueStrategy())
                    .Subscribe(ProcessMessage)
                    .CreateDefaultBinding()
                    .SetLogger(_log)
                    .Start();
            }
            catch (Exception ex)
            {
                _log.WriteErrorAsync(nameof(CashInOutQueue), nameof(Start), null, ex).Wait();
                throw;
            }
        }

        public void Stop()
        {
            _subscriber?.Stop();
        }

        private async Task ProcessMessage(CashInOutQueueMessage queueMessage)
        {
            _delayWampUpSubject.OnNewOperation(queueMessage.ClientId);    
        }
        
        public void Dispose()
        {
            Stop();
        }
    }
}
