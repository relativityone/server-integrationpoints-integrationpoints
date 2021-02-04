using System;
using Relativity.Sync.Storage;
using Relativity.Sync.SyncConfiguration.Options;
using Relativity.Sync.Utils;

#pragma warning disable 1591

namespace Relativity.Sync.SyncConfiguration
{
    public class SyncConfigurationBuilder
    {
        private readonly ISyncContext _syncContext;
        private readonly ISyncServiceManager _servicesMgr;
        private readonly ISerializer _serializer;

        public SyncConfigurationBuilder(ISyncContext syncContext, ISyncServiceManager servicesMgr)
        {
            _syncContext = syncContext;
            _servicesMgr = servicesMgr;

            _serializer = new JSONSerializer();
        }

        public ISyncJobConfigurationBuilder ConfigureRDO(RdoOptions rdoOptions)
        {
            ValidateInput(rdoOptions);
            return new SyncJobJobConfigurationBuilder(_syncContext, _servicesMgr, rdoOptions, _serializer);
        }

        private void ValidateInput(RdoOptions rdoOptions)
        {
            ValidateJobHistoryGuids(rdoOptions.JobHistory);
        }

        private void ValidateJobHistoryGuids(JobHistoryOptions jobHistoryOptions)
        {
            if (!IsValidGuid(jobHistoryOptions.JobHistoryTypeGuid))
            {
                throw GetExceptionForInvalidInput(nameof(JobHistoryOptions.JobHistoryTypeGuid),
                    nameof(JobHistoryOptions), jobHistoryOptions.JobHistoryTypeGuid.ToString());
            }

            if (!IsValidGuid(jobHistoryOptions.CompletedItemsCountGuid))
            {
                throw GetExceptionForInvalidInput(nameof(JobHistoryOptions.CompletedItemsCountGuid),
                    nameof(JobHistoryOptions), jobHistoryOptions.CompletedItemsCountGuid.ToString());
            }
            
            if (!IsValidGuid(jobHistoryOptions.FailedItemsCountGuid))
            {
                throw GetExceptionForInvalidInput(nameof(JobHistoryOptions.FailedItemsCountGuid),
                    nameof(JobHistoryOptions), jobHistoryOptions.FailedItemsCountGuid.ToString());
            }
            
            if (!IsValidGuid(jobHistoryOptions.TotalItemsCountGuid))
            {
                throw GetExceptionForInvalidInput(nameof(JobHistoryOptions.TotalItemsCountGuid),
                    nameof(JobHistoryOptions), jobHistoryOptions.TotalItemsCountGuid.ToString());
            }
        }

        private bool IsValidGuid(Guid guid)
        {
            return guid != Guid.Empty;
        }

        private InvalidSyncConfigurationException GetExceptionForInvalidInput(string propertyName, string rdoName,
            string invalidGuid)
        {
            return new InvalidSyncConfigurationException(
                $"GUID value for {rdoName}.{propertyName} is invalid: {invalidGuid}");
        }
    }
}