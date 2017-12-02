using Lykke.Job.OperationsCache.Core.Settings.JobSettings;
using Lykke.Job.OperationsCache.Core.Settings.SlackNotifications;

namespace Lykke.Job.OperationsCache.Core.Settings
{
    public class AppSettings
    {
        public OperationsCacheSettings OperationsCacheJob { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public SessionServiceSettings SessionSettings { get; set; }
        public OperationsRepositoryClientSettings OperationsRepositoryClient { get; set; }
    }

    public class SessionServiceSettings
    {
        public AzureTableSettings Sessions { get; set; }
    }

    public class AzureTableSettings
    {
        public string ConnectionString { get; set; }

        public string TableName { get; set; }
    }

    public class OperationsRepositoryClientSettings
    {
        public string ServiceUrl { get; set; }
        public int RequestTimeout { get; set; }
    }

}
