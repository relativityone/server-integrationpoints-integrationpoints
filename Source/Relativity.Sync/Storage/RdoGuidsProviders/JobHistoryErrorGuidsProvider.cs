using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage.RdoGuidsProviders
{
    internal class JobHistoryErrorGuidsProvider : IJobHistoryErrorGuidsProvider
    {
        private IConfiguration _cache;

        public JobHistoryErrorGuidsProvider(IConfiguration cache)
        {
            _cache = cache;
        }

        public Guid TypeGuid => _cache.GetFieldValue(x => x.JobHistoryErrorType);
        public Guid ErrorMessagesGuid => _cache.GetFieldValue(x => x.JobHistoryErrorErrorMessages);
        public Guid ErrorStatusGuid => _cache.GetFieldValue(x => x.JobHistoryErrorErrorStatus);
        public Guid ErrorTypeGuid => _cache.GetFieldValue(x => x.JobHistoryErrorErrorType);
        public Guid NameGuid => _cache.GetFieldValue(x => x.JobHistoryErrorName);
        public Guid SourceUniqueIdGuid => _cache.GetFieldValue(x => x.JobHistoryErrorSourceUniqueId);
        public Guid StackTraceGuid => _cache.GetFieldValue(x => x.JobHistoryErrorStackTrace);
        public Guid TimeStampGuid => _cache.GetFieldValue(x => x.JobHistoryErrorTimeStamp);
        public Guid ItemLevelErrorGuid => _cache.GetFieldValue(x => x.JobHistoryErrorItemLevelError);
        public Guid JobLevelErrorGuid => _cache.GetFieldValue(x => x.JobHistoryErrorJobLevelError);
        public Guid JobHistoryRelationGuid => _cache.GetFieldValue(x => x.JobHistoryErrorJobHistoryRelation);
        public Guid NewStatusGuid => _cache.GetFieldValue(x => x.JobHistoryErrorNewChoice);
    }
}