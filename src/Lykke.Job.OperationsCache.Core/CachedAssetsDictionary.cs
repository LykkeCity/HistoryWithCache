using Common;
using Lykke.Service.Assets.Client.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core
{
    public class CachedAssetsDictionary : CachedDataDictionary<string, Asset>
    {
        public CachedAssetsDictionary(Func<Task<Dictionary<string, Asset>>> getData, int validDataInSeconds = 300)
            : base(getData, validDataInSeconds)
        {

        }
    }
}
