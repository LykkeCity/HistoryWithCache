using Lykke.Job.OperationsCache.Core.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.OperationsRepository.Core.CashOperations
{
    public enum CashOperationType
    {
        None,
        ForwardCashOut,
        ForwardCashIn
    }

    public interface ICashInOutOperation : IBaseCashBlockchainOperation
    {
        bool IsRefund { get; set; }
        CashOperationType Type { get; set; }
    }

    public class CashInOutOperation : ICashInOutOperation
    {
        public string Id { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsHidden { get; set; }
        public string AssetId { get; set; }
        public string ClientId { get; set; }
        public double Amount { get; set; }
        public string BlockChainHash { get; set; }
        public string Multisig { get; set; }
        public string TransactionId { get; set; }
        public string AddressFrom { get; set; }
        public string AddressTo { get; set; }
        public bool? IsSettled { get; set; }
        public TransactionStates State { get; set; }
        public bool IsRefund { get; set; }
        public CashOperationType Type { get; set; }
        public double FeeSize { get; set; }        
        public FeeType FeeType { get; set; }

        public static CashInOutOperation CreateNew(string assetId, double amount)
        {
            return new CashInOutOperation
            {
                DateTime = DateTime.UtcNow,
                Amount = amount,
                AssetId = assetId
            };
        }
    }

    public interface ICashOperationsRepository
    {
        Task<string> RegisterAsync(ICashInOutOperation operation);
        Task<IEnumerable<ICashInOutOperation>> GetAsync(string clientId);
        Task<ICashInOutOperation> GetAsync(string clientId, string recordId);
        Task UpdateBlockchainHashAsync(string clientId, string id, string hash);
        Task SetBtcTransaction(string clientId, string id, string bcnTransactionId);
        Task SetIsSettledAsync(string clientId, string id, bool offchain);
        Task<IEnumerable<ICashInOutOperation>> GetByHashAsync(string blockchainHash);
        Task<IEnumerable<ICashInOutOperation>> GetByMultisigAsync(string multisig);
        Task<IEnumerable<ICashInOutOperation>> GetByMultisigsAsync(string[] multisigs);
        Task GetDataByChunksAsync(Func<IEnumerable<ICashInOutOperation>, Task> chunk);
    }
}
