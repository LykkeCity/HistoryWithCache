using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Lykke.Job.OperationsCache.Models;
using Lykke.Service.OperationsRepository.Client.Abstractions.CashOperations;

namespace Lykke.Job.OperationsCache.Services
{
    public class OperationsHistoryReader
    {
        private readonly ICashOperationsRepositoryClient _cashOperationsRepositoryClient;
        private readonly ICashOutAttemptOperationsRepositoryClient _cashOutAttemptOperationsRepositoryClient;
        private readonly ITradeOperationsRepositoryClient _tradeOperationsRepositoryClient;
        private readonly ITransferOperationsRepositoryClient _transferOperationsRepositoryClient;

        public OperationsHistoryReader(
            ICashOperationsRepositoryClient cashOperationsRepositoryClient,
            ICashOutAttemptOperationsRepositoryClient cashOutAttemptOperationsRepositoryClient,
            ITradeOperationsRepositoryClient tradeOperationsRepositoryClient,
            ITransferOperationsRepositoryClient transferOperationsRepositoryClient)
        {
            _cashOperationsRepositoryClient = cashOperationsRepositoryClient ?? throw new ArgumentNullException(nameof(cashOperationsRepositoryClient));
            _cashOutAttemptOperationsRepositoryClient = cashOutAttemptOperationsRepositoryClient ?? throw new ArgumentNullException(nameof(cashOutAttemptOperationsRepositoryClient));
            _tradeOperationsRepositoryClient = tradeOperationsRepositoryClient ?? throw new ArgumentNullException(nameof(tradeOperationsRepositoryClient));
            _transferOperationsRepositoryClient = transferOperationsRepositoryClient ?? throw new ArgumentNullException(nameof(transferOperationsRepositoryClient));
        }
        public async Task<List<HistoryEntry>> GetHistory(string clientId)
        {
            var records = new List<HistoryEntry>();
            records.AddRange((await _cashOperationsRepositoryClient.GetAsync(clientId)).Select(Mapper.MapFrom));
            records.AddRange((await _cashOutAttemptOperationsRepositoryClient.GetRequestsAsync(clientId)).Select(Mapper.MapFrom));
            records.AddRange((await _tradeOperationsRepositoryClient.GetAsync(clientId)).Select(Mapper.MapFrom));
            records.AddRange((await _transferOperationsRepositoryClient.GetAsync(clientId)).Select(Mapper.MapFrom));
            return records;
        }
    }
}
