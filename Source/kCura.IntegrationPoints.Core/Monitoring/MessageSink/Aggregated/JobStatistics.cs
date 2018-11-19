using kCura.IntegrationPoints.Common.Monitoring.Messages;

namespace kCura.IntegrationPoints.Core.Monitoring.MessageSink.Aggregated
{
	public class JobStatistics : JobMessageBase
	{
		public const string FILE_BYTES_KEY_NAME = "FileBytes";
		public const string METADATA_KEY_NAME = "MetadataBytes";
		public const string JOBSIZE_IN_BYTES_KEY_NAME = "JobSizeInBytes";
		public const string THROUGHPUT_BYTES_KEY_NAME = "ThroughputBytes";
		public const string THROUGHPUT_KEY_NAME = "Throughput";
		public const string COMPLETED_RECORDS_KEY_NAME = "CompletedRecords";
		public const string TOTAL_RECORDS_KEY_NAME = "TotalRecords";

		public JobStatus JobStatus { get; set; }

		public bool ReceivedJobstatistics { get; set; }

		public long FileBytes
		{
			get { return this.GetValueOrDefault<long>(FILE_BYTES_KEY_NAME); }
			set { CustomData[FILE_BYTES_KEY_NAME] = value; }
		}

		public long MetaBytes
		{
			get { return this.GetValueOrDefault<long>(METADATA_KEY_NAME); }
			set { CustomData[METADATA_KEY_NAME] = value; }
		}

		public long JobSizeInBytes
		{
			get { return this.GetValueOrDefault<long>(JOBSIZE_IN_BYTES_KEY_NAME); }
			set { CustomData[JOBSIZE_IN_BYTES_KEY_NAME] = value; }
		}

		public double BytesPerSecond
		{
			get { return this.GetValueOrDefault<double>(THROUGHPUT_BYTES_KEY_NAME); }
			set { CustomData[THROUGHPUT_BYTES_KEY_NAME] = value; }
		}

		public double RecordsPerSecond
		{
			get { return this.GetValueOrDefault<double>(THROUGHPUT_KEY_NAME); }
			set { CustomData[THROUGHPUT_KEY_NAME] = value; }
		}

		public long CompletedRecordsCount
		{
			get { return this.GetValueOrDefault<long>(COMPLETED_RECORDS_KEY_NAME); }
			set { CustomData[COMPLETED_RECORDS_KEY_NAME] = value; }
		}

		public long TotalRecordsCount
		{
			get { return this.GetValueOrDefault<long>(TOTAL_RECORDS_KEY_NAME); }
			set { CustomData[TOTAL_RECORDS_KEY_NAME] = value; }
		}
	}
}