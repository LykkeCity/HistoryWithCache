using System;
using System.Collections.Generic;
using Lykke.SettingsReader.Attributes;

namespace Lykke.Job.OperationsCache.Settings.JobSettings
{
    public class OperationsCacheSettings
    {
        [Optional]
        public string CacheInstanceName { get; set; }
        public TimeSpan ExpirationPeriod { get; set; }
        public DbSettings Db { get; set; }
        public int ItemsPerPage { get; set; }
        public List<string> ExcludeClientIdList { get; set; }
        public TimeSpan UpdateDelayPeriod { get; set; }
    }
}
