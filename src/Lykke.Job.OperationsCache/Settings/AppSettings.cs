using Lykke.Job.OperationsCache.Settings.ClientSettings;
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
}
