using Lykke.Job.OperationsCache.Core.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Lykke.Service.OperationsRepository.Core.CashOperations
{
    public sealed class PaymentSystem
    {
        public PaymentSystem(string value)
        {
            Value = value;
        }

        public string Value { get; }

        public override string ToString()
        {
            return Value;
        }

        public static implicit operator string(PaymentSystem paymentSystem)
        {
            return paymentSystem.ToString();
        }
    }

    public enum CashOutRequestStatus
    {
        ClientConfirmation = 4,
        Pending = 0,
        RequestForDocs = 7,
        Confirmed = 1,
        Declined = 2,
        CanceledByClient = 5,
        CanceledByTimeout = 6,
        Processed = 3,
    }

    public enum CashOutVolumeSize
    {
        Unknown, High, Low
    }

    public enum CashOutRequestTradeSystem
    {
        Spot,
        Margin
    }

    public interface ICashOutRequest : IBaseCashOperation
    {
        string PaymentSystem { get; }
        string PaymentFields { get; }
        string BlockchainHash { get; }
        CashOutRequestStatus Status { get; }
        TransactionStates State { get; }
        CashOutRequestTradeSystem TradeSystem { get; }
        string AccountId { get; }
        CashOutVolumeSize VolumeSize { get; }
        string PreviousId { get; }
    }

    public class SwiftCashOutRequest : ICashOutRequest
    {
        public string Id { get; set; }
        public string ClientId { get; set; }
        public string AssetId { get; set; }
        public string PaymentSystem { get; set; }
        public string PaymentFields { get; set; }
        public string BlockchainHash { get; set; }
        public CashOutRequestStatus Status { get; set; }
        public TransactionStates State { get; set; }
        public CashOutRequestTradeSystem TradeSystem { get; set; }
        public double Amount { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsHidden { get; set; }
        public string AccountId { get; set; }
        public CashOutVolumeSize VolumeSize { get; set; }
        public string PreviousId { get; set; }
        public double FeeSize { get; set; }
        public FeeType FeeType { get; set; }
    }

    public interface ICashOutBaseRepository
    {
        Task<IEnumerable<ICashOutRequest>> GetRequestsAsync(string clientId);
        Task<ICashOutRequest> GetAsync(string clientId, string id);
    }
}
