using System;

namespace Lykke.Job.OperationsCache.Core.Settings.JobSettings
{
    public class OperationsCacheSettings
    {
        public TimeSpan ExpirationPeriod { get; set; }
        public DbSettings Db { get; set; }
        public int ItemsPerPage { get; set; }
    }
}
