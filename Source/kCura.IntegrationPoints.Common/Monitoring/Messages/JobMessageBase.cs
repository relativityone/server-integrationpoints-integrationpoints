using System.Collections.Generic;
using Relativity.DataTransfer.MessageService;
using Relativity.DataTransfer.MessageService.MetricsManager.APM;

namespace kCura.IntegrationPoints.Common.Monitoring.Messages
{
	public abstract class JobMessageBase : IMessage, IMetricMetadata
	{
		private const string _PROVIDER_KEY_NAME = "Provider";
		private const string _JOB_ID_KEY_NAME = "JobID";

		public string Provider
		{
			get { return this.GetValueOrDefault<string>(_PROVIDER_KEY_NAME); }
			set { CustomData[_PROVIDER_KEY_NAME] = value; }
		}

		// ReSharper disable once InconsistentNaming
		public string JobID
		{
			get { return this.GetValueOrDefault<string>(_JOB_ID_KEY_NAME) ?? string.Empty; }
			set { CustomData[_JOB_ID_KEY_NAME] = value; }
		}

		public string CorrelationID { get; set; }
		public int WorkspaceID { get; set; }
		public string UnitOfMeasure { get; set; }
		
		public Dictionary<string, object> CustomData { get; set; } = new Dictionary<string, object>();
	}
}