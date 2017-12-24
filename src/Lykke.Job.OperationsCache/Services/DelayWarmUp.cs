using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Lykke.Job.OperationsCache.Services
{
    public class DelayWarmUp : IDelayWarmUp
    {
        private readonly IHistoryCache _historyCache;
        private readonly TimeSpan _delayPeriod;
        private readonly IList<string> _excludeList;
        private readonly ClientSessionsRepository _clientSessionsRepository;

        public DelayWarmUp(
            IHistoryCache historyCache, 
            TimeSpan delayPeriod, 
            IList<string> excludeList,
            ClientSessionsRepository clientSessionsRepository)
        {
            _historyCache = historyCache;
            _delayPeriod = delayPeriod;
            _excludeList = excludeList;
            _clientSessionsRepository = clientSessionsRepository;
        }

        public async Task OnNewOperation(string clientId)
        {
            if (_excludeList.Contains(clientId))
                return;

            var activeSessions = await _clientSessionsRepository.GetClientsIds();
            if (!activeSessions.Contains(clientId))
                return;

            await Task.Delay(_delayPeriod);

            await WarmUp(clientId);
        }

        private async Task WarmUp(string clientId)
        {
            await _historyCache.WarmUp(clientId, true);
        }
    }
}
