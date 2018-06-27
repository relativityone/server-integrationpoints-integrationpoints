using System;
using System.Collections.Generic;
using Relativity.DataTransfer.MessageService;
using Relativity.DataTransfer.MessageService.MetricsManager.APM;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport
{
	public class ImportJobStatisticsMessage : IMetricMetadata, IMessage
	{
		private const string _JOBID_KEY_NAME = "JobID";
		private const string _FILE_BYTES_KEY_NAME = "FileBytes";
		private const string _METADATA_KEY_NAME = "MetadataBytes";

		public string Provider { get; set; }
		public string CorellationID { get; set; }
		public Dictionary<string, object> CustomData { get; set; } = new Dictionary<string, object>();
		public int WorkspaceID { get; set; }
		public string UnitOfMeasure { get; set; }

		public string JobID
		{
			get { return GetValueOrDefault<string>(_JOBID_KEY_NAME); }
			set { CustomData[_JOBID_KEY_NAME] = value; }
		}

		public long FileBytes
		{
			get { return GetValueOrDefault<long>(_FILE_BYTES_KEY_NAME); }
			set { CustomData[_FILE_BYTES_KEY_NAME] = value; }
		}

		public long MetaBytes
		{
			get { return GetValueOrDefault<long>(_METADATA_KEY_NAME); }
			set { CustomData[_METADATA_KEY_NAME] = value; }
		}

		// TODO: refactor this to use MetricMetadataExtensions from IntegrationPoints.Core
		public T GetValueOrDefault<T>(string key)
		{
			object value;
			return CustomData.TryGetValue(key, out value) ? (T)value : default(T);
		}
	}
}