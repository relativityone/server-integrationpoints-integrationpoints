using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage.RdoGuidsProviders
{
    internal class JobHistoryRdoGuidsProvider : IJobHistoryRdoGuidsProvider
    {
        private readonly IConfiguration _cache;

        public JobHistoryRdoGuidsProvider(IConfiguration cache)
        {
            _cache = cache;
        }

        public Guid TypeGuid => _cache.GetFieldValue(x => x.JobHistoryType);

        public Guid StatusGuid => _cache.GetFieldValue(x => x.JobHistoryStatusField);

        public Guid CompletedItemsFieldGuid => _cache.GetFieldValue(x => x.JobHistoryCompletedItemsField);

        public Guid FailedItemsFieldGuid => _cache.GetFieldValue(x => x.JobHistoryGuidFailedField);

        public Guid TotalItemsFieldGuid => _cache.GetFieldValue(x => x.JobHistoryGuidTotalField);

        public Guid DestinationWorkspaceInformationGuid => _cache.GetFieldValue(x => x.JobHistoryDestinationWorkspaceInformationField);

        public Guid JobIdGuid => _cache.GetFieldValue(x => x.JobHistoryJobIdField);

        public Guid StartTimeGuid => _cache.GetFieldValue(x => x.JobHistoryStartTimeField);

        public Guid EndTimeGuid => _cache.GetFieldValue(x => x.JobHistoryEndTimeField);
    }
}
