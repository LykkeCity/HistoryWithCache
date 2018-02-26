using System;
using System.Collections.Generic;

namespace Core.Settings
{
    public class OperationsCacheSettings
    {
        public int MaxHistoryLengthPerClient { get; set; }
        public int SaveHistoryLengthPerClient { get; set; }
        public string CacheInstanceName { get; set; }
        public TimeSpan ExpirationPeriod { get; set; }
        public DbSettings Db { get; set; }
        public int ItemsPerPage { get; set; }
        public List<string> ExcludeClientIdList { get; set; }
        public TimeSpan UpdateDelayPeriod { get; set; }        
    }
}
