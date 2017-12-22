﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Core.BitCoin;
using Core.CashOperations;
using Core.Exchange;
using Core.Offchain;
using Lykke.Job.OperationsCache.Handlers.Models;
using Lykke.Job.OperationsCache.Services;
using Lykke.Job.OperationsCache.Settings;
using Lykke.Job.OperationsCache.Settings.JobSettings;
using Lykke.RabbitMqBroker;
using Lykke.RabbitMqBroker.Subscriber;
using Lykke.Service.Assets.Client;
using Lykke.Service.Assets.Client.Models;
using Lykke.Service.OperationsRepository.Core.CashOperations;
using Microsoft.AspNetCore.JsonPatch.Operations;

namespace Lykke.Job.OperationsCache.Handlers
{
    public class LimitTradeQueue : IQueueSubscriber
    {
#if DEBUG
        private const string QueueName = "transactions.limit-trades-dev.cache";
        private const bool QueueDurable = false;
#else
        private const string QueueName = "transactions.limit-trades.cache";
        private const bool QueueDurable = true;
#endif
                
        private readonly ILog _log;
        
        private readonly RabbitMqSettings _rabbitConfig;
        private readonly IDelayWampUpSubject _delayWampUpSubject;
        private RabbitMqSubscriber<LimitQueueItem> _subscriber;

        public LimitTradeQueue(
            RabbitMqSettings config,
            IDelayWampUpSubject delayWampUpSubject,
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
                IsDurable = QueueDurable
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
                _delayWampUpSubject.OnNewOperation(limitOrder.Order.ClientId);
            }            
        }

        
        public void Dispose()
        {
            Stop();
        }
    }
}
