using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Service.OperationsCache.AutorestClient;
using Lykke.Service.OperationsCache.Client.Models;
using System.Collections.Generic;

namespace Lykke.Service.OperationsCache.Client
{
    public class OperationsCacheClient : IOperationsCacheClient, IDisposable
    {
        private readonly ILog _log;
        private IOperationsCacheAPI _apiClient;

        public OperationsCacheClient(string serviceUrl, ILog log)
        {
            _log = log;
            _apiClient = new OperationsCacheAPI(new Uri(serviceUrl));
        }

        public void Dispose()
        {
            if (_apiClient == null)
                return;
            _apiClient.Dispose();
            _apiClient = null;
        }

        public async Task<IEnumerable<HistoryClientEntry>> GetHistoryByClientId(string clientId)
        {
            var operations = await _apiClient.GetHistoryAsync(clientId);
            return operations == null 
                ? new List<HistoryClientEntry>() 
                : operations.Select(x => x.FromApiModel());
        }
    }
}
