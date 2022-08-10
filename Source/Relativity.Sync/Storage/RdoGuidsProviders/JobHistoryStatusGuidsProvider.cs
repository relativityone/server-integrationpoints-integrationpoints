using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage.RdoGuidsProviders
{
    internal class JobHistoryStatusGuidsProvider : IJobHistoryStatusProvider
    {
        private readonly IConfiguration _cache;

        public JobHistoryStatusGuidsProvider(IConfiguration cache)
        {
            _cache = cache;
        }

        public Guid CompletedGuid => _cache.GetFieldValue(x => x.JobHistoryStatusCompleted);

        public Guid CompletedWithErrorsGuid => _cache.GetFieldValue(x => x.JobHistoryStatusCompletedWithErrors);

        public Guid JobFailedGuid => _cache.GetFieldValue(x => x.JobHistoryStatusJobFailed);

        public Guid ProcessingGuid => _cache.GetFieldValue(x => x.JobHistoryStatusProcessing);

        public Guid StoppedGuid => _cache.GetFieldValue(x => x.JobHistoryStatusStopped);

        public Guid StoppingGuid => _cache.GetFieldValue(x => x.JobHistoryStatusStopping);

        public Guid SuspendedGuid => _cache.GetFieldValue(x => x.JobHistoryStatusSuspended);

        public Guid SuspendingGuid => _cache.GetFieldValue(x => x.JobHistoryStatusSuspending);

        public Guid ValidatingGuid => _cache.GetFieldValue(x => x.JobHistoryStatusValidating);

        public Guid ValidationFailedGuid => _cache.GetFieldValue(x => x.JobHistoryStatusValidationFailed);
    }
}
