using System;
using System.Collections.Async;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Common;
using Common.Log;
using Lykke.Job.OperationsCache.Services;
using System.Collections.Generic;

namespace Lykke.Job.OperationsCache.PeriodicalHandlers
{
    public class MyPeriodicalHandler : TimerPeriod
    {
        private readonly ILog _log;
        private readonly IHistoryCache _historyCache;
        private readonly ClientSessionsRepository _clientSessionsRepository;
        private static bool _inProcess;
        private IList<string> _excludeList;

        public MyPeriodicalHandler(
            ClientSessionsRepository clientSessionsRepository,
            IHistoryCache historyCache,
            ILog log,
            TimeSpan expirationPeriod,
            IList<string> excludeList) :
            base(nameof(MyPeriodicalHandler), (int)expirationPeriod.TotalMilliseconds, log)
        {
            _log = log ?? throw new ArgumentNullException(nameof(log));
            _clientSessionsRepository = clientSessionsRepository ?? throw new ArgumentNullException(nameof(_clientSessionsRepository));
            _historyCache = historyCache ?? throw new ArgumentNullException(nameof(historyCache));
            _excludeList = excludeList;
        }

        public override async Task Execute()
        {
            if (_inProcess)
                return;

            try
            {
                _inProcess = true;

                var timestamp = DateTime.UtcNow;
                var clientsIds = (await _clientSessionsRepository.GetClientsIds()).Where(id => !_excludeList.Contains(id)).ToList();

                await _log.WriteInfoAsync(GetComponentName(), "Updating cache", $"Processing {clientsIds.Count} active clients.");
                foreach (var clientId in clientsIds)
                {
                    await _historyCache.WarmUp(clientId, true).ConfigureAwait(false);
                }

                await _log.WriteInfoAsync(GetComponentName(), "Updating cache", $"Processed in {(DateTime.UtcNow - timestamp).TotalSeconds} seconds.");
            }
            finally
            {

                _inProcess = false;
            }
        }
    }
}
