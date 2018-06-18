using System.Collections.Generic;
using Relativity.DataTransfer.MessageService.MetricsManager.APM;

namespace kCura.IntegrationPoints.Core.Monitoring.NumberOfRecords.Messages
{
	public class JobApmThroughputMessage : JobMessageBase, IMetricMetadata
	{
		private const string _JOB_ID_KEY_NAME = "JobID";
		private const string _METADATA_THROUGHPUT_KEY_NAME = "MetadataThroughput";
		private const string _FILE_THROUGHPUT = "FileThroughput";
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

		public double MetadataThroughput
		{
			get { return this.GetValueOrDefault<double>(_METADATA_THROUGHPUT_KEY_NAME); }
			set { CustomData[_METADATA_THROUGHPUT_KEY_NAME] = value; }
		}

		public double FileThroughput
		{
			get { return this.GetValueOrDefault<double>(_FILE_THROUGHPUT); }
			set { CustomData[_FILE_THROUGHPUT] = value; }
		}
	}
}