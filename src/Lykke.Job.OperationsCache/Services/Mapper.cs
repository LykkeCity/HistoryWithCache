﻿using Lykke.Job.OperationsCache.Models;
using Newtonsoft.Json;
using Lykke.Service.OperationsRepository.AutorestClient.Models;

namespace Lykke.Job.OperationsCache.Services
{
    public static class Mapper
    {
        public static HistoryEntry MapFrom(CashInOutOperation source)
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

        public static HistoryEntry MapFrom(CashOutAttemptEntity source)
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

        public static HistoryEntry MapFrom(ClientTrade source)
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

        public static HistoryEntry MapFrom(TransferEvent source)
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
    }
}
