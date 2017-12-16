namespace Lykke.Job.OperationsCache.Settings.JobSettings
{
    public class RabbitMqSettings
    {
        public string ConnectionString { get; set; }

        public string ExternalHost { get; set; }

        public int Port { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public string ExchangeSwap { get; set; }
        public string ExchangeLimit { get; set; }

        public string ExchangeCashOperation { get; set; }
        public string ExchangeTransfer { get; set; }
        public string ExchangeSwapOperation { get; set; }

        public string ExchangeEthereumCashIn { get; set; }
    }
}
