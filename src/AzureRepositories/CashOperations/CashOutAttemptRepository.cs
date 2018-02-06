using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AzureStorage;
using Common;
using Core.CashOperations;
using Microsoft.WindowsAzure.Storage.Table;

namespace AzureRepositories.CashOperations
{
    public class CashOutAttemptEntity : CashOutBaseEntity
    {
        public static class PendingRecords
        {
            public static string GeneratePartition(string clientId)
            {
                return clientId;
            }

            public static string GenerateRowKey(string requestId)
            {
                return requestId;
            }

            public static CashOutAttemptEntity Create<T>(ICashOutRequest request, PaymentSystem paymentSystem, T paymentFields, CashOutRequestTradeSystem tradeSystem)
            {
                var reqId = Guid.NewGuid().ToString("N");
                var entity = CreateEntity(request, paymentSystem, paymentFields, tradeSystem);
                entity.PartitionKey = GeneratePartition(request.ClientId);
                entity.RowKey = GenerateRowKey(reqId);
                entity.Status = request.Status;
                entity.PreviousId = request.PreviousId;

                return entity;
            }
        }

        public static class HistoryRecords
        {
            public static string GeneratePartition()
            {
                return "Processed";
            }

            public static CashOutAttemptEntity Create<T>(ICashOutRequest request, PaymentSystem paymentSystem,
                T paymentFields, CashOutRequestTradeSystem tradeSystem)
            {
                var entity = CreateEntity(request, paymentSystem, paymentFields, tradeSystem);
                entity.PartitionKey = GeneratePartition();

                return entity;
            }
        }

        public static CashOutAttemptEntity CreateEntity<T>(ICashOutRequest request, PaymentSystem paymentSystem, T paymentFields, CashOutRequestTradeSystem tradeSystem)
        {
            var dt = DateTime.UtcNow;
            return new CashOutAttemptEntity
            {
                AssetId = request.AssetId,
                Amount = request.Amount,
                ClientId = request.ClientId,
                PaymentSystem = paymentSystem,
                PaymentFields = paymentFields.ToJson(),
                DateTime = dt,
                State = request.State,
                TradeSystem = tradeSystem.ToString(),
                AccountId = request.AccountId,
                VolumeSize = request.VolumeSize,
                PreviousId = request.PreviousId
            };
        }
    }

    public class CashOutAttemptRepository : ICashOutAttemptRepository
    {
        readonly INoSQLTableStorage<CashOutAttemptEntity> _tableStorage;

        public CashOutAttemptRepository(INoSQLTableStorage<CashOutAttemptEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<string> InsertRequestAsync<T>(ICashOutRequest request, PaymentSystem paymentSystem, T paymentFields, CashOutRequestTradeSystem tradeSystem)
        {
            var entity = CashOutAttemptEntity.PendingRecords.Create(request, paymentSystem, paymentFields, tradeSystem);
            await _tableStorage.InsertAsync(entity);
            return entity.RowKey;
        }

        public async Task<IEnumerable<ICashOutRequest>> GetAllAttempts()
        {
            return await _tableStorage.GetDataAsync();
        }

        public Task SetBlockchainHash(string clientId, string requestId, string hash)
        {
            return _tableStorage.MergeAsync(CashOutAttemptEntity.PendingRecords.GeneratePartition(clientId),
                requestId, entity =>
                {
                    entity.BlockchainHash = hash;
                    entity.State = TransactionStates.SettledOnchain;
                    return entity;
                });
        }

        public Task SetIsSettledOffchain(string clientId, string requestId)
        {
            return _tableStorage.MergeAsync(CashOutAttemptEntity.PendingRecords.GeneratePartition(clientId),
                requestId, entity =>
                {
                    entity.State = TransactionStates.SettledOffchain;
                    return entity;
                });
        }

        public async Task<ICashOutRequest> SetPending(string clientId, string requestId)
        {
            return await _tableStorage.MergeAsync(CashOutAttemptEntity.PendingRecords.GeneratePartition(clientId),
                requestId, entity =>
                {
                    entity.Status = CashOutRequestStatus.Pending;
                    return entity;
                });
        }

        public async Task<ICashOutRequest> SetDocsRequested(string clientId, string requestId)
        {
            return await _tableStorage.MergeAsync(CashOutAttemptEntity.PendingRecords.GeneratePartition(clientId),
                requestId, entity =>
                {
                    entity.Status = CashOutRequestStatus.RequestForDocs;
                    return entity;
                });
        }

        public async Task<ICashOutRequest> SetConfirmed(string clientId, string requestId)
        {
            return await _tableStorage.MergeAsync(CashOutAttemptEntity.PendingRecords.GeneratePartition(clientId),
                requestId, entity =>
                {
                    entity.Status = CashOutRequestStatus.Confirmed;
                    return entity;
                });
        }

        public async Task<ICashOutRequest> SetDeclined(string clientId, string requestId)
        {
            return await ChangeStatus(clientId, requestId, CashOutRequestStatus.Declined);
        }

        public async Task<ICashOutRequest> SetCanceledByClient(string clientId, string requestId)
        {
            return await ChangeStatus(clientId, requestId, CashOutRequestStatus.CanceledByClient);
        }

        public async Task<ICashOutRequest> SetCanceledByTimeout(string clientId, string requestId)
        {
            return await ChangeStatus(clientId, requestId, CashOutRequestStatus.CanceledByTimeout);
        }

        public async Task<ICashOutRequest> SetProcessed(string clientId, string requestId)
        {
            return await ChangeStatus(clientId, requestId, CashOutRequestStatus.Processed);
        }

        public async Task<ICashOutRequest> SetHighVolume(string clientId, string requestId)
        {
            return await _tableStorage.MergeAsync(CashOutAttemptEntity.PendingRecords.GeneratePartition(clientId),
                requestId, entity =>
                {
                    entity.VolumeSize = CashOutVolumeSize.High;
                    return entity;
                });
        }

        private async Task<ICashOutRequest> ChangeStatus(string clientId, string requestId, CashOutRequestStatus status)
        {
            var entity = await _tableStorage.DeleteAsync(CashOutAttemptEntity.PendingRecords.GeneratePartition(clientId),
                CashOutAttemptEntity.PendingRecords.GenerateRowKey(requestId));

            entity.PartitionKey = CashOutAttemptEntity.HistoryRecords.GeneratePartition();
            entity.Status = status;
            entity.PreviousId = requestId;

            return await _tableStorage.InsertAndGenerateRowKeyAsDateTimeAsync(entity, entity.DateTime);
        }

        public async Task<IEnumerable<ICashOutRequest>> GetHistoryRecordsAsync(DateTime @from, DateTime to)
        {
            to = to.Date.AddDays(1);
            var partitionKey = CashOutAttemptEntity.HistoryRecords.GeneratePartition();
            return await _tableStorage.WhereAsync(partitionKey, from, to, ToIntervalOption.ExcludeTo);
        }

        public async Task<IEnumerable<ICashOutRequest>> GetRequestsAsync(string clientId)
        {
            return await _tableStorage.GetDataAsync(CashOutAttemptEntity.PendingRecords.GeneratePartition(clientId));
        }

        public async Task<ICashOutRequest> GetAsync(string clientId, string requestId)
        {
            return await _tableStorage.GetDataAsync(CashOutAttemptEntity.PendingRecords.GeneratePartition(clientId), requestId);
        }
        
        public async Task<IEnumerable<ICashOutRequest>> GetRelatedRequestsAsync(string requestId)
        {
            var rowKeyCond = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.Equal, requestId);
            var previousIdCond = TableQuery.GenerateFilterCondition(nameof(ICashOutRequest.PreviousId), QueryComparisons.Equal, requestId);
            var query = new TableQuery<CashOutAttemptEntity>
            {
                FilterString = TableQuery.CombineFilters(rowKeyCond, TableOperators.Or, previousIdCond)
            };

            var requests = await _tableStorage.WhereAsync(query);
            return requests;
        }
    }
}
