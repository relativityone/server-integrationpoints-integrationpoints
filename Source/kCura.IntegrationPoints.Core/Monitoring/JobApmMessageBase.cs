using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Monitoring.NumberOfRecords.Messages;
using Relativity.DataTransfer.MessageService.MetricsManager.APM;

namespace kCura.IntegrationPoints.Core.Monitoring
{
	public class JobApmMessageBase : JobMessageBase, IMetricMetadata
	{
		private const string _JOB_ID_KEY_NAME = "JobID";

		public string CorellationID { get; set; }
		public Dictionary<string, object> CustomData { get; set; } = new Dictionary<string, object>();
		public int WorkspaceID { get; set; }
		public string UnitOfMeasure { get; set; }

		// ReSharper disable once InconsistentNaming
		public string JobID
		{
			get { return this.GetValueOrDefault<string>(_JOB_ID_KEY_NAME) ?? string.Empty; }
			set { CustomData[_JOB_ID_KEY_NAME] = value; }
		}
	}
}