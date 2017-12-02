using Lykke.Job.OperationsCache.Core.Settings.JobSettings;
using Lykke.Job.OperationsCache.Core.Settings.SlackNotifications;

namespace Lykke.Job.OperationsCache.Core.Settings
{
    public class AppSettings
    {
        public OperationsCacheSettings OperationsCacheJob { get; set; }
        public SlackNotificationsSettings SlackNotifications { get; set; }
    }
}