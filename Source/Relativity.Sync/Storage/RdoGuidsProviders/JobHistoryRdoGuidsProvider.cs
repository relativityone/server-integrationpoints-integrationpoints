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

        public Guid TypeGuid => _valueGetter(SyncRdoGuids.JobHistoryTypeGuid);
        public Guid CompletedItemsFieldGuid => _valueGetter(SyncRdoGuids.JobHistoryCompletedItemsFieldGuid);
        public Guid FailedItemsFieldGuid => _valueGetter(SyncRdoGuids.JobHistoryFailedItemsFieldGuid);
        public Guid TotalItemsFieldGuid => _valueGetter(SyncRdoGuids.JobHistoryTotalItemsFieldGuid);
        public Guid DestinationWorkspaceInformationGuid =>  _valueGetter(SyncRdoGuids.JobHistoryDestinationWorkspaceInformationGuid);
    }
}