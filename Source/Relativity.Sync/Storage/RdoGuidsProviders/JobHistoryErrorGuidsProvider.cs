using System;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Storage.RdoGuidsProviders
{
    internal class JobHistoryErrorGuidsProvider : IJobHistoryErrorGuidsProvider
    {
        private readonly Func<Guid, Guid> _valueGetter;

        public JobHistoryErrorGuidsProvider(Func<Guid, Guid> valueGetter)
        {
            _valueGetter = valueGetter;
        }

        public Guid TypeGuid => _valueGetter(x => x.JobHistoryErrorType);
        public Guid ErrorMessagesGuid => _valueGetter(x => x.JobHistoryErrorErrorMessages);
        public Guid ErrorStatusGuid => _valueGetter(x => x.JobHistoryErrorErrorStatus);
        public Guid ErrorTypeGuid => _valueGetter(x => x.JobHistoryErrorErrorType);
        public Guid NameGuid => _valueGetter(x => x.JobHistoryErrorName);
        public Guid SourceUniqueIdGuid => _valueGetter(x => x.JobHistoryErrorSourceUniqueId);
        public Guid StackTraceGuid => _valueGetter(x => x.JobHistoryErrorStackTrace);
        public Guid TimeStampGuid => _valueGetter(x => x.JobHistoryErrorTimeStamp);
        public Guid ItemLevelErrorGuid => _valueGetter(x => x.JobHistoryErrorItemLevelError);
        public Guid JobLevelErrorGuid => _valueGetter(x => x.JobHistoryErrorJobLevelError);
        public Guid JobHistoryRelationGuid => _valueGetter(x => x.JobHistoryErrorJobHistoryRelation);
        public Guid NewStatusGuid => _valueGetter(x => x.JobHistoryErrorNewChoice);
    }
}