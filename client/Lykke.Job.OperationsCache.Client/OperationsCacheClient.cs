using System;
using System.Linq;
using System.Threading.Tasks;
using Common.Log;
using Lykke.Job.OperationsCache.AutorestClient;
using Lykke.Job.OperationsCache.Client.Models;
using System.Collections.Generic;

namespace Lykke.Job.OperationsCache.Client
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
            var response = await _apiClient.GetHistoryWithHttpMessagesAsync(clientId);

            var operations = response.Body;

            return operations == null ? new List<HistoryClientEntry>() : operations.Select(x => x.FromApiModel());
        }

        public Task RemoveCashoutIfExists(string clientId, string operationId)
        {
            return _apiClient.DeleteCashOperationWithHttpMessagesAsync(clientId, operationId);
        }
    }
}
