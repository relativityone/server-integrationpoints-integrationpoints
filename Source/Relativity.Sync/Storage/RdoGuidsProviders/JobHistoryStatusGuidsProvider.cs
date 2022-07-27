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

        public Guid CompletedGuid
        {
            get { return _cache.GetFieldValue(x => x.JobHistoryStatusCompleted); }
        }

        public Guid CompletedWithErrorsGuid
        {
            get { return _cache.GetFieldValue(x => x.JobHistoryStatusCompletedWithErrors); }
        }

        public Guid JobFailedGuid
        {
            get { return _cache.GetFieldValue(x => x.JobHistoryStatusJobFailed); }
        }

        public Guid ProcessingGuid
        {
            get { return _cache.GetFieldValue(x => x.JobHistoryStatusProcessing); }
        }

        public Guid StoppedGuid
        {
            get { return _cache.GetFieldValue(x => x.JobHistoryStatusStopped); }
        }

        public Guid StoppingGuid
        {
            get { return _cache.GetFieldValue(x => x.JobHistoryStatusStopping); }
        }

        public Guid SuspendedGuid
        {
            get { return _cache.GetFieldValue(x => x.JobHistoryStatusSuspended); }
        }

        public Guid SuspendingGuid
        {
            get { return _cache.GetFieldValue(x => x.JobHistoryStatusSuspending); }
        }

        public Guid ValidatingGuid
        {
            get { return _cache.GetFieldValue(x => x.JobHistoryStatusValidating); }
        }

        public Guid ValidationFailedGuid
        {
            get { return _cache.GetFieldValue(x => x.JobHistoryStatusValidationFailed); }
        }
    }
}
