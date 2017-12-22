﻿using System;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.OperationsCache.Handlers.Models;
using Lykke.Job.OperationsCache.Services;
using Lykke.Job.OperationsCache.Settings.JobSettings;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;

namespace Lykke.Job.OperationsCache.Handlers
{
    public class TradeQueue : IQueueSubscriber
    {
        private const string QueueName = "transactions.trades.cache";

        private readonly ILog _log;

        private readonly RabbitMqSettings _rabbitConfig;
        private readonly IDelayWampUpSubject _delayWampUpSubject;        
        private RabbitMqSubscriber<TradeQueueItem> _subscriber;        

        public TradeQueue(RabbitMqSettings rabbitConfig, IDelayWampUpSubject delayWampUpSubject, ILog log)
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
                ExchangeName = _rabbitConfig.ExchangeSwap,
                DeadLetterExchangeName = $"{_rabbitConfig.ExchangeTransfer}.cache.dlx",
                RoutingKey = "",
                IsDurable = true
            };

            try
            {
                _subscriber = new RabbitMqSubscriber<TradeQueueItem>(settings, new ResilientErrorHandlingStrategy(_log, settings, TimeSpan.FromSeconds(5), int.MaxValue))
                    .SetMessageDeserializer(new JsonMessageDeserializer<TradeQueueItem>())
                    .SetMessageReadStrategy(new MessageReadWithTemporaryQueueStrategy())
                    .Subscribe(ProcessMessage)
                    .CreateDefaultBinding()
                    .SetLogger(_log)
                    .Start();
            }
            catch (Exception ex)
            {
                _log.WriteErrorAsync(nameof(TradeQueue), nameof(Start), null, ex).Wait();
                throw;
            }
        }

        public void Stop()
        {            
            _subscriber?.Stop();
        }

        private async Task ProcessMessage(TradeQueueItem queueMessage)
        {
            _delayWampUpSubject.OnNewOperation(queueMessage.Order.ClientId);
        }

        public void Dispose()
        {
            Stop();
        }        
    }
}