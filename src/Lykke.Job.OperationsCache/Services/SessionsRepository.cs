using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AzureStorage;
using JetBrains.Annotations;
using Lykke.Service.Session.Client;

namespace Lykke.Job.OperationsCache.Services
{
    [UsedImplicitly]
    public class ClientSessionsRepository
    {
        private readonly IClientSessionsClient _sessionsClient;

        public ClientSessionsRepository(IClientSessionsClient sessionsClient)
        {
            _sessionsClient = sessionsClient;
        }

        public async Task<IEnumerable<string>> GetClientsIds()
        {
            return await _sessionsClient.GetActiveClientIdsAsync();
        }

    }
}
