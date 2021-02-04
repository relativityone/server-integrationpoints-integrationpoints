using System;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Storage.RdoGuidsProviders
{
    internal class JobHistoryErrorStatusGuidProvider : IJobHistoryErrorStatusGuidProvider
    {
        private readonly Func<Guid, Guid> _valueGetter;

        public JobHistoryErrorStatusGuidProvider(Func<Guid, Guid> valueGetter)
        {
            _valueGetter = valueGetter;
        }

        public Guid New => _valueGetter(SyncConfigurationRdo.JobHistoryErrorNewChoiceGuid);
        public Guid Expired => _valueGetter(SyncConfigurationRdo.JobHistoryErrorExpiredChoiceGuid);
        public Guid InProgress => _valueGetter(SyncConfigurationRdo.JobHistoryErrorInProgressChoiceGuid);
        public Guid Retried => _valueGetter(SyncConfigurationRdo.JobHistoryErrorRetriedChoiceGuid);
    }
}