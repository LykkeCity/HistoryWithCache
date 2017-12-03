using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.OperationsRepository.Core.CashOperations
{
    public interface IClientTrade : IBaseCashBlockchainOperation
    {
        string LimitOrderId { get; }
        string MarketOrderId { get; }
        double Price { get; }
        DateTime? DetectionTime { get; set; }
        int Confirmations { get; set; }
        string OppositeLimitOrderId { get; set; }
        bool IsLimitOrderResult { get; set; }
    }


    public class ClientTrade : IClientTrade
    {
        public string Id { get; set; }
        public string ClientId { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsHidden { get; set; }
        public string LimitOrderId { get; set; }
        public string MarketOrderId { get; set; }
        public double Amount { get; set; }
        public string AssetId { get; set; }
        public string BlockChainHash { get; set; }
        public string Multisig { get; set; }
        public string TransactionId { get; set; }
        public string AddressFrom { get; set; }
        public string AddressTo { get; set; }
        public bool? IsSettled { get; set; }
        public TransactionStates State { get; set; }
        public double Price { get; set; }
        public DateTime? DetectionTime { get; set; }
        public int Confirmations { get; set; }
        public string OppositeLimitOrderId { get; set; }
        public bool IsLimitOrderResult { get; set; }
    }

    public interface IClientTradesRepository
    {
        Task<IClientTrade[]> SaveAsync(params IClientTrade[] clientTrades);
        Task<IEnumerable<IClientTrade>> GetAsync(string clientId);

        Task<IEnumerable<IClientTrade>> GetAsync(DateTime from, DateTime to);

        Task<IClientTrade> GetAsync(string clientId, string recordId);
        Task UpdateBlockChainHashAsync(string clientId, string recordId, string hash);
        Task SetDetectionTimeAndConfirmations(string clientId, string recordId, DateTime detectTime, int confirmations);
        Task SetBtcTransactionAsync(string clientId, string recordId, string btcTransactionId);
        Task SetIsSettledAsync(string clientId, string id, bool offchain);
        Task<IEnumerable<IClientTrade>> GetByMultisigAsync(string multisig);
        Task<IEnumerable<IClientTrade>> GetByMultisigsAsync(string[] multisigs);
        Task<IEnumerable<IClientTrade>> GetByOrderAsync(string orderId);

        Task<IEnumerable<IClientTrade>> ScanByDtAsync(DateTime from, DateTime to);
        Task ScanByDtAsync(Func<IEnumerable<IClientTrade>, Task> chunk, DateTime from, DateTime to);
        Task GetDataByChunksAsync(Func<IEnumerable<IClientTrade>, Task> chunk);
    }

    public static class Utils
    {
        // Supports old records previously created by ME
        public static string GenerateRecordId(DateTime dt)
        {
            return $"{dt.Year}{dt.Month:00}{dt.Day:00}{dt.Hour:00}{dt.Minute:00}_{Guid.NewGuid()}";
        }
    }

}
