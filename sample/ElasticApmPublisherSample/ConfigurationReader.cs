using System;
using System.Collections.Generic;
using Elastic.Apm.Config;
using Elastic.Apm.Logging;
using Microsoft.Extensions.Configuration;

namespace ElasticApmPublisherSample
{
    public class ConfigurationReader: AbstractConfigurationReader, IConfigurationReader
    {
        private static class Keys
        {
            internal const string LogLevel = "ELASTIC_APM_LOG_LEVEL";
            internal const string ServerUrls = "ELASTIC_APM_SERVER_URLS";
            internal const string ServiceName = "ELASTIC_APM_SERVICE_NAME";
            internal const string SecretToken = "ELASTIC_APM_SECRET_TOKEN";
            internal const string CaptureHeaders = "ELASTIC_APM_CAPTURE_HEADERS";
            internal const string TransactionSampleRate = "ELASTIC_APM_TRANSACTION_SAMPLE_RATE";
        }

        private const string Origin = "appsettings.json";
        private readonly IConfiguration _configuration;
        public TimeSpan FlushInterval { get; }
        public LogLevel LogLevel { get; set; }
        public int MaxBatchEventCount { get { return 1;} }
        public int MaxQueueEventCount { get { return 1;} }
        public double MetricsIntervalInMilliseconds { get; }
        public IReadOnlyList<Uri> ServerUrls { get; set; }
        public string ServiceName { get; set; }
        public string ServiceVersion { get; }
        public double SpanFramesMinDurationInMilliseconds { get; }
        public int StackTraceLimit { get; }
        public string SecretToken { get; set; }
        public string CaptureBody { get; }
        public List<string> CaptureBodyContentTypes { get; }
        public bool CaptureHeaders { get; set; }
        public bool CentralConfig { get; }
        public string Environment { get; }
        public double TransactionSampleRate { get; set; }
        public int TransactionMaxSpans { get; }
        public double MetricsIntervalInMillisecond { get; }

        private ConfigurationKeyValue Read(string key) => Kv(key, _configuration[key], Origin);

        public ConfigurationReader(IConfiguration configuration) : base(default, default)
        {
            _configuration = configuration;
            SetLogLevel();
            SetServerUrls();
            SetServiceName();
            SetSecretToken();
            SetCaptureHeaders();
            SetTransactionSampleRate();
        }

        private void SetLogLevel()
        {
            var key = Read(Keys.LogLevel);
            LogLevel = ParseLogLevel(key);
        }

        private void SetServerUrls()
        {
            var key = Read(Keys.ServerUrls);
            ServerUrls = ParseServerUrls(key);
        }

        private void SetServiceName()
        {
            var key = Read(Keys.ServiceName);
            ServiceName = ParseServiceName(key);
        }

        private void SetSecretToken()
        {
            var key = Read(Keys.SecretToken);
            SecretToken = ParseSecretToken(key);
        }

        private void SetCaptureHeaders()
        {
            var key = Read(Keys.CaptureHeaders);
            CaptureHeaders = ParseCaptureHeaders(key);
        }

        private void SetTransactionSampleRate()
        {
            var key = Read(Keys.TransactionSampleRate);
            TransactionSampleRate = ParseTransactionSampleRate(key);
        }
    }
}