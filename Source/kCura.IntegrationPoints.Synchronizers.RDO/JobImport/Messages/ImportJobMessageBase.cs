using System.Collections.Generic;
using Relativity.DataTransfer.MessageService;
using Relativity.DataTransfer.MessageService.MetricsManager.APM;

namespace kCura.IntegrationPoints.Synchronizers.RDO.JobImport
{
	public class ImportJobMessageBase: IMetricMetadata, IMessage
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

		// TODO: refactor this to use MetricMetadataExtensions from IntegrationPoints.Core
		public T GetValueOrDefault<T>(string key)
		{
			object value;
			return CustomData.TryGetValue(key, out value) ? (T)value : default(T);
		}
	}
}