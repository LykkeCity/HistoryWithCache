﻿namespace Lykke.Job.OperationsCache.Core.Settings.JobSettings
{
    public class DbSettings
    {
        public string LogsConnString { get; set; }
        public string ClientTradesConnString { get; set; }
        public string CashOperationsConnString { get; set; }
        public string TransferConnString { get; set; }
        public string CashOutAttemptConnString { get; set; }
        public string LimitTradesConnString { get; set; }
        public string ClientPersonalInfoConnString { get; set; }
    }
}
