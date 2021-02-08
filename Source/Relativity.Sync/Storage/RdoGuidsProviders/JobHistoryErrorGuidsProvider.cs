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

        public Guid TypeGuid => _valueGetter(SyncConfigurationRdo.JobHistoryErrorTypeGuid);
        public Guid ErrorMessagesGuid => _valueGetter(SyncConfigurationRdo.JobHistoryErrorErrorMessagesGuid);
        public Guid ErrorStatusGuid => _valueGetter(SyncConfigurationRdo.JobHistoryErrorErrorStatusGuid);
        public Guid ErrorTypeGuid => _valueGetter(SyncConfigurationRdo.JobHistoryErrorErrorTypeGuid);
        public Guid NameGuid => _valueGetter(SyncConfigurationRdo.JobHistoryErrorNameGuid);
        public Guid SourceUniqueIdGuid => _valueGetter(SyncConfigurationRdo.JobHistoryErrorSourceUniqueIdGuid);
        public Guid StackTraceGuid => _valueGetter(SyncConfigurationRdo.JobHistoryErrorStackTraceGuid);
        public Guid TimeStampGuid => _valueGetter(SyncConfigurationRdo.JobHistoryErrorTimeStampGuid);
        public Guid ItemLevelErrorGuid => _valueGetter(SyncConfigurationRdo.JobHistoryErrorItemLevelErrorGuid);
        public Guid JobLevelErrorGuid => _valueGetter(SyncConfigurationRdo.JobHistoryErrorJobLevelErrorGuid);
        public Guid JobHistoryRelationGuid => _valueGetter(SyncConfigurationRdo.JobHistoryErrorJobHistoryRelationGuid);
        public Guid NewStatusGuid => _valueGetter(SyncConfigurationRdo.JobHistoryErrorNewChoiceGuid);
    }
}