using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Common;

namespace Lykke.Job.OperationsCache.Core
{
    public class CachedSessionsDictionary : CachedDataDictionary<string, string>
    {
        public CachedSessionsDictionary(Func<Task<Dictionary<string, string>>> getData, int validDataInSeconds = 300) : base(getData, validDataInSeconds)
        {
        }
    }
}
