using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;

namespace AzureRepositories.Sessions
{
    public class ClientSessionsRepository
    {
        private const string ByTokenPartitionKey = "Sess";
        private readonly INoSQLTableStorage<ClientSessionEntity> _tableStorage;

        public ClientSessionsRepository(INoSQLTableStorage<ClientSessionEntity> tableStorage)
        {
            _tableStorage = tableStorage;
        }

        public async Task<IEnumerable<ClientSessionEntity>> GetAll()
        {
            return await _tableStorage.GetDataAsync(ByTokenPartitionKey);
        }

        public async Task<IEnumerable<string>> GetClientsIds()
        {
            return (await _tableStorage.GetDataAsync(ByTokenPartitionKey)).Select(x => x.ClientId);
        }

    }
}
