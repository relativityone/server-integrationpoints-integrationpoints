﻿namespace kCura.IntegrationPoints.Contracts
{
    public static class Constants
    {
        public const string SPECIAL_NATIVE_FILE_LOCATION_FIELD = "fb83020a-1853-4a14-8e92-accb6dbd2ef1";
        public const string SPECIAL_FOLDERPATH_FIELD = "d1d9ccdd-9773-428f-8465-4a03909192e4";
        public const string SPECIAL_FILE_NAME_FIELD = "67F1BEA1-67CB-498E-A99D-1781D43D98AF";
        public const string SPECIAL_SOURCEWORKSPACE_FIELD = "036DB373-5724-4C72-A073-375106DE5E73";
        public const string SPECIAL_SOURCEWORKSPACE_FIELD_NAME = "Relativity Source Case";
        public const string SPECIAL_SOURCEJOB_FIELD = "4F632A3F-68CF-400E-BD29-FD364A5EBE58";
        public const string SPECIAL_SOURCEJOB_FIELD_NAME = "Relativity Source Job";
        public const string SOURCEWORKSPACE_CASEID_FIELD_NAME = "Source Workspace Artifact ID";
        public const string SOURCEWORKSPACE_CASENAME_FIELD_NAME = "Source Workspace Name";
        public const string SOURCEWORKSPACE_NAME_FIELD_NAME = "Name";
        public const string SOURCEJOB_NAME_FIELD_NAME = "Name";
        public const string SOURCEJOB_JOBHISTORYID_FIELD_NAME = "Job History Artifact ID";
        public const string SOURCEJOB_JOBHISTORYNAME_FIELD_NAME = "Job History Name";
        public const string SPECIAL_NATIVE_FILE_LOCATION_FIELD_NAME = "NATIVE_FILE_PATH_001";
        public const string SPECIAL_FOLDERPATH_FIELD_NAME = "REL_FOLDER_PATH_001";
        public const string SPECIAL_FILE_NAME_FIELD_NAME = "REL_FILE_NAME_001";
        public const string SCHEDULE_QUEUE_INSTANCE_SETTING_SECTION = "kCura.ScheduleQueue.Core";
        public const string INTEGRATION_POINT_INSTANCE_SETTING_SECTION = "kCura.IntegrationPoints";
	    public const string REMOVE_ERROR_BATCH_SIZE_INSTANCE_SETTING_NAME = "RemoveErrorsFromScratchTableBatchSize";
        public const string WEB_API_PATH = "WebAPIPath";
        public const string SOURCEPROVIDER_ARTIFACTID_FIELD = "4A091F69-D750-441C-A4F0-24C990D208AE";
        public const string RELATIVITY_PROVIDER_GUID = "423b4d43-eae9-4e14-b767-17d629de4bb2";
        public const char MULTI_VALUE_DELIMITER = ';';
        public const char NESTED_VALUE_DELIMITER = '/';

        internal static class IntegrationPoints
        {
            public const string APP_DOMAIN_DATA_CONNECTION_STRING = "IntegrationPointsConnectionString";
            public const string APP_DOMAIN_SUBSYSTEM_NAME = "IntegrationPoints";
            public const string APPLICATION_GUID_STRING = "DCF6E9D1-22B6-4DA3-98F6-41381E93C30C";
	        public const string UNABLE_TO_INSTANTIATE_PROVIDER_FACTORY = "Unable to instantiate the provider factory. Check the class implementing ProviderFactoryBase.";
			public const string TOO_MANY_PROVIDER_FACTORIES = "Too many provider factories have been found. Make sure there is only one implementation of ProviderFactoryBase.";
		}
    }
}