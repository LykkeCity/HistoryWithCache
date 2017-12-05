using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Core.CashOperations;
using Lykke.Job.OperationsCache.Models;
using Lykke.Service.OperationsRepository.Core.CashOperations;
using Core.BitCoin;

namespace Lykke.Job.OperationsCache.Services.OperationsHistory
{
    public class OperationsHistoryRepoReader : IOperationsHistoryReader
    {
        private readonly ICashOperationsRepository _cashOperationsRepository;
        private readonly IClientTradesRepository _clientTradesRepository;
        private readonly ITransferEventsRepository _transferEventsRepository;
        private readonly ICashOutAttemptRepository _cashOutAttemptRepository;
        private readonly ILimitTradeEventsRepository _limitTradeEventsRepository;
        private readonly IWalletCredentialsRepository _walletCredentialsRepository;

        public OperationsHistoryRepoReader(
            ICashOperationsRepository cashOperationsRepository,
            IClientTradesRepository clientTradesRepository,
            ITransferEventsRepository transferEventsRepository,
            ICashOutAttemptRepository cashOutAttemptRepository,
            ILimitTradeEventsRepository limitTradeEventsRepository,
            IWalletCredentialsRepository walletCredentialsRepository)
        {
            _cashOperationsRepository = cashOperationsRepository ?? throw new ArgumentNullException(nameof(cashOperationsRepository));
            _clientTradesRepository = clientTradesRepository ?? throw new ArgumentNullException(nameof(clientTradesRepository));
            _transferEventsRepository = transferEventsRepository ?? throw new ArgumentNullException(nameof(transferEventsRepository));
            _cashOutAttemptRepository = cashOutAttemptRepository ?? throw new ArgumentNullException(nameof(cashOutAttemptRepository));
            _limitTradeEventsRepository = limitTradeEventsRepository ?? throw new ArgumentNullException(nameof(limitTradeEventsRepository));
            _walletCredentialsRepository = walletCredentialsRepository ?? throw new ArgumentNullException(nameof(walletCredentialsRepository));
        }

        public async Task<List<HistoryEntry>> GetHistory(string clientId)
        {
            var records = new List<HistoryEntry>();
            var walletCreds = await _walletCredentialsRepository.GetAsync(clientId);
            var multisig = walletCreds?.MultiSig;

            if (!string.IsNullOrWhiteSpace(multisig))
            {
                records.AddRange((await _cashOperationsRepository.GetByMultisigAsync(multisig)).Select(RepoMapper.MapFrom));
                records.AddRange((await _clientTradesRepository.GetByMultisigAsync(multisig)).Select(RepoMapper.MapFrom));
                records.AddRange((await _transferEventsRepository.GetByMultisigAsync(multisig)).Select(RepoMapper.MapFrom));
            }
            else
            {
                records.AddRange((await _cashOperationsRepository.GetAsync(clientId)).Select(RepoMapper.MapFrom));
                records.AddRange((await _clientTradesRepository.GetAsync(clientId)).Select(RepoMapper.MapFrom));
                records.AddRange((await _transferEventsRepository.GetAsync(clientId)).Select(RepoMapper.MapFrom));
            }

            records.AddRange((await _cashOutAttemptRepository.GetRequestsAsync(clientId)).Select(RepoMapper.MapFrom));
            records.AddRange((await _limitTradeEventsRepository.GetEventsAsync(clientId)).Select(RepoMapper.MapFrom));

            return records;
        }
    }
}
