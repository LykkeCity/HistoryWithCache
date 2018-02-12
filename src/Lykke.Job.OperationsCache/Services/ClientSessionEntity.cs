using System;
using Microsoft.WindowsAzure.Storage.Table;

namespace Lykke.Job.OperationsCache.Services
{
    public class ClientSessionEntity : TableEntity
    {
        public string ClientId { get; set; }
        public string SessionToken => RowKey;
        public string ClientInfo { get; set; }
        public DateTime Registered { get; set; }
        public DateTime LastAction { get; set; }
        public string PartnerId { get; set; }

        internal void Update(string clientInfo, string partnerId, DateTime lastAction)
        {
            ClientInfo = clientInfo;
            PartnerId = partnerId;
            LastAction = lastAction;
        }

        public static ClientSessionEntity Create(string clientId, string sessionToken, string clientInfo, string partnerId, DateTime registered, string partitionKey)
        {
            return new ClientSessionEntity
            {
                ClientId = clientId,
                RowKey = sessionToken,
                ClientInfo = clientInfo,
                PartnerId = partnerId,
                Registered = registered,
                LastAction = registered,
                PartitionKey = partitionKey
            };
        }
    }
}
