namespace kCura.IntegrationPoints.Common.Monitoring.Messages
{
    public class JobProgressMessage : JobMessageBase
    {
        private const string _METADATA_THROUGHPUT_KEY_NAME = "MetadataThroughput";
        private const string _FILE_THROUGHPUT = "FileThroughput";

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