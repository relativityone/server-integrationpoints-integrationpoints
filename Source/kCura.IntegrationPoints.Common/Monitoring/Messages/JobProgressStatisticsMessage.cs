using System.Collections.Generic;

namespace kCura.IntegrationPoints.Common.Monitoring.Messages
{
    public class JobProgressStatisticsMessage : JobProgressMessage
    {
        private const string _AVERAGE_METADATA_THROUGHPUT_KEY_NAME = "AverageMetadataThroughput";
        private const string _AVERAGE_FILE_THROUGHPUT = "AverageFileThroughput";

        public JobProgressStatisticsMessage(JobProgressMessage source)
        {
            this.Provider = source.Provider;
            this.JobID = source.JobID;
            this.CorrelationID = source.CorrelationID;
            this.WorkspaceID = source.WorkspaceID;
            this.UnitOfMeasure = source.UnitOfMeasure;
            this.CustomData = new Dictionary<string, object>(source.CustomData);
        }

        public double? AverageMetadataThroughput
        {
            get { return this.GetValueOrDefault<double?>(_AVERAGE_METADATA_THROUGHPUT_KEY_NAME); }
            set { CustomData[_AVERAGE_METADATA_THROUGHPUT_KEY_NAME] = value; }
        }

        public double? AverageFileThroughput
        {
            get { return this.GetValueOrDefault<double?>(_AVERAGE_FILE_THROUGHPUT); }
            set { CustomData[_AVERAGE_FILE_THROUGHPUT] = value; }
        }
    }
}