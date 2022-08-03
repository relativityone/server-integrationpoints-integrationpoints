namespace kCura.IntegrationPoints.Common.Monitoring.Messages
{
    public class JobStatisticsMessage : JobMessageBase
    {
        private const string _FILE_BYTES_KEY_NAME = "FileBytes";
        private const string _METADATA_KEY_NAME = "MetadataBytes";
        private const string _JOBSIZE_IN_BYTES_KEY_NAME = "JobSizeInBytes";

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
    }
}