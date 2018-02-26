using Core.Settings;
using Core.Settings.Sessions;
using Core.Settings.SlackNotifications;
using Lykke.Job.OperationsCache.Settings.JobSettings;

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
