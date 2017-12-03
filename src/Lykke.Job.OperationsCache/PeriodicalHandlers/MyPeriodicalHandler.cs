﻿using System;
using System.Collections.Async;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Job.OperationsCache.Services;

namespace Lykke.Job.OperationsCache.PeriodicalHandlers
{
    public class MyPeriodicalHandler : TimerPeriod
    {
        private readonly ILog _log;
        private readonly IHistoryCache _historyCache;
        private readonly ClientSessionsRepository _clientSessionsRepository;
        private static bool _inProcess;

        public MyPeriodicalHandler(
            ClientSessionsRepository clientSessionsRepository,
            IHistoryCache historyCache,
            ILog log,
            TimeSpan expirationPeriod) :
            base(nameof(MyPeriodicalHandler), (int)expirationPeriod.TotalMilliseconds, log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _clientSessionsRepository = clientSessionsRepository ?? throw new ArgumentNullException(nameof(_clientSessionsRepository));
            _historyCache = historyCache ?? throw new ArgumentNullException(nameof(historyCache));
        }

        public override async Task Execute()
        {
            if (_inProcess)
                return;

            try
            {
                _inProcess = true;

                var timestamp = DateTime.UtcNow;
                var clientsIds = (await _clientSessionsRepository.GetClientsIds()).ToList();
                await clientsIds.ParallelForEachAsync(async clientId =>
                {
                    await _historyCache.WarmUp(clientId, true).ConfigureAwait(false);
                }).ConfigureAwait(false);

                var memSize = Process.GetCurrentProcess().PrivateMemorySize64 >> 20;
                await _log.WriteInfoAsync(GetComponentName(), "Updating cache", $"Processed {clientsIds.Count} active clients in {(DateTime.UtcNow - timestamp).TotalSeconds} seconds. PrivateMemorySize: {memSize} Mb");
            }
            finally
            {

                _inProcess = false;
            }
        }
    }
}
