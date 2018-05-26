using Lykke.Job.OperationsCache.Settings.JobSettings;
using Lykke.Job.OperationsCache.Settings.SlackNotifications;
using Lykke.Service.Session.Client;

namespace Lykke.Job.OperationsCache.Settings
{
    public class AppSettings
    {
        public OperationsCacheSettings OperationsCacheJob { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
        public SessionServiceClient SessionServiceClient { get; set; }
        public AssetsServiceClientSettings AssetsServiceClient { get; set; }
        public RabbitMqSettings RabbitMq { get; set; }
        public RedisSettings RedisSettings { get; set; }
    }

    public class SessionServiceClient
    {
        public string SessionServiceUrl { set; get; }
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
