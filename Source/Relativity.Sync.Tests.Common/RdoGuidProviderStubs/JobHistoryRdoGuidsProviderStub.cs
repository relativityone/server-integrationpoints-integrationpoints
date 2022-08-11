using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Common.RdoGuidProviderStubs
{
    internal class JobHistoryRdoGuidsProviderStub : IJobHistoryRdoGuidsProvider
    {
        public Guid TypeGuid { get; set; }
        public Guid CompletedItemsFieldGuid { get; set; }
        public Guid FailedItemsFieldGuid { get; set; }
        public Guid TotalItemsFieldGuid { get; set; }
        public Guid DestinationWorkspaceInformationGuid { get; set; }
        public Guid JobIdGuid { get; set; }
        public Guid StartTimeGuid { get; set; }
        public Guid EndTimeGuid { get; set; }
        public Guid StatusGuid { get; set; }
    }
}