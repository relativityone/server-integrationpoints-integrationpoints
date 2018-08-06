using kCura.IntegrationPoints.Core.Monitoring.NumberOfRecords.Messages;

namespace kCura.IntegrationPoints.Core.Monitoring.Sinks.Aggregated
{
	public class JobStatistics : JobMessageBase
	{
		public const string _FILE_BYTES_KEY_NAME = "FileBytes";
		public const string _METADATA_KEY_NAME = "MetadataBytes";
		public const string _JOBSIZE_IN_BYTES_KEY_NAME = "JobSizeInBytes";
		public const string _THROUGHPUT_BYTES_KEY_NAME = "ThroughputBytes";
		public const string _THROUGHPUT_KEY_NAME = "Throughput";
		public const string _COMPLETED_RECORDS_KEY_NAME = "CompletedRecords";
		public const string _TOTAL_RECORDS_KEY_NAME = "TotalRecords";

		public JobStatus JobStatus { get; set; }

		public bool ReceivedJobstatistics { get; set; }

		public long FileBytes
		{
			get { return this.GetValueOrDefault<long>(_FILE_BYTES_KEY_NAME); }
			set { CustomData[_FILE_BYTES_KEY_NAME] = value; }
		}

		public long MetaBytes
		{
			get { return this.GetValueOrDefault<long>(_METADATA_KEY_NAME); }
			set { CustomData[_METADATA_KEY_NAME] = value; }
		}

		public long JobSizeInBytes
		{
			get { return this.GetValueOrDefault<long>(_JOBSIZE_IN_BYTES_KEY_NAME); }
			set { CustomData[_JOBSIZE_IN_BYTES_KEY_NAME] = value; }
		}

		public double BytesPerSecond
		{
			get { return this.GetValueOrDefault<double>(_THROUGHPUT_BYTES_KEY_NAME); }
			set { CustomData[_THROUGHPUT_BYTES_KEY_NAME] = value; }
		}

		public double RecordsPerSecond
		{
			get { return this.GetValueOrDefault<double>(_THROUGHPUT_KEY_NAME); }
			set { CustomData[_THROUGHPUT_KEY_NAME] = value; }
		}

		public long CompletedRecordsCount
		{
			get { return this.GetValueOrDefault<long>(_COMPLETED_RECORDS_KEY_NAME); }
			set { CustomData[_COMPLETED_RECORDS_KEY_NAME] = value; }
		}

		public long TotalRecordsCount
		{
			get { return this.GetValueOrDefault<long>(_TOTAL_RECORDS_KEY_NAME); }
			set { CustomData[_TOTAL_RECORDS_KEY_NAME] = value; }
		}
	}
}