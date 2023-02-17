using System;
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
        public const string START_TIME_KEY_NAME = "StartTime";
        public const string END_TIME_KEY_NAME = "EndTime";
        public const string DURATION_SECONDS_KEY_NAME = "DurationSeconds";
        public const string OVERALL_THROUGHPUT_BYTES_KEY_NAME = "OverallThroughputBytes";
        public const string AVERAGE_FILE_THROUGHPUT_NAME = "AverageFileThroughput";
        public const string AVERAGE_METADATA_THROUGHPUT_NAME = "AverageMetadataThroughput";
        public const string LAST_THROUGHPUT_CHECK_NAME = "LastThroughputCheck";

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

        public DateTime StartTime
        {
            get { return this.GetValueOrDefault<DateTime>(START_TIME_KEY_NAME); }
            set { CustomData[START_TIME_KEY_NAME] = value; }
        }

        public DateTime EndTime
        {
            get { return this.GetValueOrDefault<DateTime>(END_TIME_KEY_NAME); }
            set { CustomData[END_TIME_KEY_NAME] = value; }
        }

        public double DurationSeconds
        {
            get { return this.GetValueOrDefault<double>(DURATION_SECONDS_KEY_NAME); }
            set { CustomData[DURATION_SECONDS_KEY_NAME] = value; }
        }

        public double OverallThroughputBytes
        {
            get { return this.GetValueOrDefault<double>(OVERALL_THROUGHPUT_BYTES_KEY_NAME); }
            set { CustomData[OVERALL_THROUGHPUT_BYTES_KEY_NAME] = value; }
        }

        public double? AverageFileThroughput
        {
            get { return this.GetValueOrDefault<double?>(AVERAGE_FILE_THROUGHPUT_NAME); }
            set { CustomData[AVERAGE_FILE_THROUGHPUT_NAME] = value; }
        }

        public double? AverageMetadataThroughput
        {
            get { return this.GetValueOrDefault<double?>(AVERAGE_METADATA_THROUGHPUT_NAME); }
            set { CustomData[AVERAGE_METADATA_THROUGHPUT_NAME] = value; }
        }

        public DateTime LastThroughputCheck
        {
            get { return this.GetValueOrDefault<DateTime>(LAST_THROUGHPUT_CHECK_NAME); }
            set { CustomData[LAST_THROUGHPUT_CHECK_NAME] = value; }
        }
    }
}
