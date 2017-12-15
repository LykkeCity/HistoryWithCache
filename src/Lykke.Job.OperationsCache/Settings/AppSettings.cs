
using Lykke.Job.OperationsCache.Settings.JobSettings;
using Lykke.Job.OperationsCache.Settings.SlackNotifications;

namespace Lykke.Job.OperationsCache.Settings
{
    public class AppSettings
    {
        public OperationsCacheSettings OperationsCacheJob { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public SessionServiceSettings SessionSettings { get; set; }
        public AssetsServiceClientSettings AssetsServiceClient { get; set; }
        public RabbitMqSettings RabbitMq { get; set; }
        public RedisSettings RedisSettings { get; set; }
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

    public class AssetsServiceClientSettings
    {
        public string ServiceUrl { get; set; }
    }

    public class RedisSettings
    {
        public string Configuration { get; set; }
    }
}
