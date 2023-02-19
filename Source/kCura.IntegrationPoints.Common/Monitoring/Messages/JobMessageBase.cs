using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Relativity.DataTransfer.MessageService;
using Relativity.DataTransfer.MessageService.MetricsManager.APM;

namespace kCura.IntegrationPoints.Common.Monitoring.Messages
{
    public abstract class JobMessageBase : IMessage, IMetricMetadata
    {
        public string Provider
        {
            get { return Get<string>(); }
            set { Set(value); }
        }

        // ReSharper disable once InconsistentNaming
        public string JobID
        {
            get { return Get<string>(); }
            set { Set(value); }
        }

        public string CorrelationID { get; set; }

        public int WorkspaceID { get; set; }

        public string UnitOfMeasure { get; set; }

        public Dictionary<string, object> CustomData { get; set; } = new Dictionary<string, object>();

        protected void Set(object value, [CallerMemberName] string key = "")
        {
            ValidateKey(key);
            CustomData[key] = value;
        }

        protected T Get<T>([CallerMemberName] string key = "")
        {
            ValidateKey(key);
            return this.GetValueOrDefault<T>(key);
        }

        private void ValidateKey(string key)
        {
            if (string.IsNullOrEmpty(key))
            {
                throw new InvalidOperationException("Key cannot be empty");
            }
        }
    }
}
