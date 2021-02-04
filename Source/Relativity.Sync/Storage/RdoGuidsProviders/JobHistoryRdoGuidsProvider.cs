using System;
using Relativity.Sync.Configuration;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Storage.RdoGuidsProviders
{
    internal class JobHistoryRdoGuidsProvider : IJobHistoryRdoGuidsProvider
    {
        private readonly Func<Guid, Guid> _valueGetter;

        public JobHistoryRdoGuidsProvider(Func<Guid, Guid> valueGetter)
        {
            _valueGetter = valueGetter;
        }

        public Guid TypeGuid => _valueGetter(SyncConfigurationRdo.JobHistoryTypeGuid);
        public Guid CompletedItemsFieldGuid => _valueGetter(SyncConfigurationRdo.JobHistoryCompletedItemsFieldGuid);
        public Guid FailedItemsFieldGuid => _valueGetter(SyncConfigurationRdo.JobHistoryFailedItemsFieldGuid);
        public Guid TotalItemsFieldGuid => _valueGetter(SyncConfigurationRdo.JobHistoryTotalItemsFieldGuid);
        public Guid DestinationWorkspaceInformationGuid =>  _valueGetter(SyncConfigurationRdo.JobHistoryDestinationWorkspaceInformationGuid);
    }
}