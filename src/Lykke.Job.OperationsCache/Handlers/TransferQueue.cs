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
    public class TransferQueue : IQueueSubscriber
    {
        private const string QueueName = "transactions.transfer.cache";

        private readonly ILog _log;        

        private readonly RabbitMqSettings _rabbitConfig;
        private readonly IDelayWarmUp _delayWampUpSubject;
        private RabbitMqSubscriber<TransferQueueMessage> _subscriber;        

        public TransferQueue(RabbitMqSettings rabbitConfig, IDelayWarmUp delayWampUpSubject, ILog log)
        {
            _rabbitConfig = rabbitConfig;
            _delayWampUpSubject = delayWampUpSubject;            
            _log = log;                    
        }

        public void Start()
        {
            var settings = new RabbitMqSubscriptionSettings
            {
                ConnectionString = _rabbitConfig.ConnectionString,
                QueueName = QueueName,
                ExchangeName = _rabbitConfig.ExchangeTransfer,
                DeadLetterExchangeName = $"{_rabbitConfig.ExchangeTransfer}.cache.dlx",
                RoutingKey = "",
                IsDurable = true
            };

            try
            {                
                _subscriber = new RabbitMqSubscriber<TransferQueueMessage>(settings, new ResilientErrorHandlingStrategy(_log, settings, TimeSpan.FromSeconds(5), int.MaxValue))
                    .SetMessageDeserializer(new JsonMessageDeserializer<TransferQueueMessage>())
                    .SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy())
                    .Subscribe(ProcessMessage)
                    .CreateDefaultBinding()
                    .SetLogger(_log)
                    .Start();
            }
            catch (Exception ex)
            {
                _log.WriteErrorAsync(nameof(TransferQueue), nameof(Start), null, ex).Wait();
                throw;
            }
        }

        public void Stop()
        {
            _subscriber?.Stop();            
        }

        private async Task ProcessMessage(TransferQueueMessage queueMessage)
        {
            _delayWampUpSubject.OnNewOperation(queueMessage.FromClientId);
            _delayWampUpSubject.OnNewOperation(queueMessage.ToClientid);
        }

        public void Dispose()
        {                      
            Stop();
        }        
    }
}
