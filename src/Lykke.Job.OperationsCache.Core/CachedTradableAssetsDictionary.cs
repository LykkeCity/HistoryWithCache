﻿using Common;
using Lykke.Service.Assets.Client.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Core
{
    public class CachedTradableAssetsDictionary : CachedDataDictionary<string, Asset>
    {
        public CachedTradableAssetsDictionary(Func<Task<Dictionary<string, Asset>>> getData, int validDataInSeconds = 300)
            : base(getData, validDataInSeconds)
        {

        }
    }
}
