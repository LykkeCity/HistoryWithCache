using Core.Settings;
using Core.Settings.SlackNotifications;
using Lykke.Service.OperationsCache.Settings.ServiceSettings;

namespace Lykke.Service.OperationsCache.Settings
{
    public class AppSettings
    {
        public OperationsCacheServiceSettings OperationsCacheService { get; set; }

        public SlackNotificationsSettings SlackNotifications { get; set; }
        public OperationsCacheSettings OperationsCacheJob { get; set; }
        public AssetsServiceClientSettings AssetsServiceClient { get; set; }
        public RedisSettings RedisSettings { get; set; }
    }
}
