using Core.CashOperations;
using Lykke.Job.OperationsCache.Models;
using Lykke.Service.OperationsRepository.Core.CashOperations;
using Newtonsoft.Json;

namespace Lykke.Job.OperationsCache.Services.OperationsHistory
{
    public static class RepoMapper
    {
        public static HistoryEntry MapFrom(ICashInOutOperation source)
        {
            return new HistoryEntry
            {
                Id = source.Id,
                ClientId = source.ClientId,
                Currency = source.AssetId,
                DateTime = source.DateTime,
                OpType = "CashInOut",
                Amount = source.Amount,
                CustomData = JsonConvert.SerializeObject(source)
            };
        }

        public static HistoryEntry MapFrom(ICashOutRequest source)
        {
            return new HistoryEntry
            {
                Id = source.Id,
                ClientId = source.ClientId,
                Currency = source.AssetId,
                OpType = "CashOutAttempt",
                DateTime = source.DateTime,
                Amount = source.Amount,
                CustomData = JsonConvert.SerializeObject(source)
            };
        }

        public static HistoryEntry MapFrom(IClientTrade source)
        {
            return new HistoryEntry
            {
                Id = source.Id,
                ClientId = source.ClientId,
                Amount = source.Amount,
                Currency = source.AssetId,
                DateTime = source.DateTime,
                OpType = "ClientTrade",
                CustomData = JsonConvert.SerializeObject(source)
            };
        }

        public static HistoryEntry MapFrom(ITransferEvent source)
        {
            return new HistoryEntry
            {
                Id = source.Id,
                ClientId = source.ClientId,
                DateTime = source.DateTime,
                Amount = source.Amount,
                Currency = source.AssetId,
                OpType = "TransferEvent",
                CustomData = JsonConvert.SerializeObject(source)
            };
        }

        public static HistoryEntry MapFrom(ILimitTradeEvent source)
        {
            return new HistoryEntry
            {
                Id = source.Id,
                ClientId = source.ClientId,
                DateTime = source.CreatedDt,
                Amount = source.Volume,
                Currency = source.AssetId,
                OpType = "LimitTradeEvent",
                CustomData = JsonConvert.SerializeObject(source)
            };
        }
    }
}
