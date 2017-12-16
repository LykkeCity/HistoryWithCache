using System;
using System.Collections.Generic;

namespace Lykke.Job.OperationsCache.Settings.JobSettings
{
    public class OperationsCacheSettings
    {
        public TimeSpan ExpirationPeriod { get; set; }
        public DbSettings Db { get; set; }
        public int ItemsPerPage { get; set; }
        public List<string> ExcludeClientIdList { get; set; }
        public TimeSpan UpdateDelayPeriod { get; set; }
    }
}
