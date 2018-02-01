using System;
using Common;
using Microsoft.WindowsAzure.Storage.Table;
using Lykke.Service.OperationsRepository.Core.CashOperations;
using Lykke.Job.OperationsCache.Core.Domain;

namespace Lykke.Service.OperationsRepository.AzureRepositories.CashOperations
{
    public abstract class CashOutBaseEntity : TableEntity, ICashOutRequest
    {
        public string Id => RowKey;
        public string ClientId { get; set; }
        public string AssetId { get; set; }
        public string PaymentSystem { get; set; }
        public string PaymentFields { get; set; }
        public string BlockchainHash { get; set; }
        public string TradeSystem { get; set; }
        CashOutRequestTradeSystem ICashOutRequest.TradeSystem => TradeSystem.ParseEnum(CashOutRequestTradeSystem.Spot);
        public string AccountId { get; set; }

        public CashOutRequestStatus Status
        {
            get => (CashOutRequestStatus)StatusVal;
            set => StatusVal = (int)value;
        }

        public TransactionStates State
        {
            get => (TransactionStates)StateVal;
            set => StateVal = (int)value;
        }

        public double Amount { get; set; }
        public DateTime DateTime { get; set; }
        public bool IsHidden { get; set; }

        public int StatusVal { get; set; }
        public int StateVal { get; set; }

        public CashOutVolumeSize VolumeSize { get; set; }
        public string VolumeText
        {
            get { return VolumeSize.ToString(); }
            set
            {
                CashOutVolumeSize volumeSize;
                if (Enum.TryParse(value, out volumeSize))
                    VolumeSize = volumeSize;
                else
                    VolumeSize = CashOutVolumeSize.Unknown;
            }
        }

        public string PreviousId { get; set; }

        public double FeeSize { get; set; }
        public FeeType FeeType { get; set; }
        public string FeeTypeText
        {
            get => FeeType.ToString();

            set => FeeType = Enum.TryParse(value, out FeeType tmpType) ? tmpType : FeeType.Unknown;
        }
    }
}
