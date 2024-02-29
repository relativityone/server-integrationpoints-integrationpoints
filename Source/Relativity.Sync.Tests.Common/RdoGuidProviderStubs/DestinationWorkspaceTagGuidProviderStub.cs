using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Tests.Common.RdoGuidProviderStubs
{
    internal class DestinationWorkspaceTagGuidProviderStub : IDestinationWorkspaceTagGuidProvider
    {
        public Guid TypeGuid { get; set; }

        public Guid NameGuid { get; set; }

        public Guid DestinationWorkspaceNameGuid { get; set; }

        public Guid DestinationWorkspaceArtifactIdGuid { get; set; }

        public Guid DestinationInstanceNameGuid { get; set; }

        public Guid DestinationInstanceArtifactIdGuid { get; set; }

        public Guid JobHistoryOnDocumentGuid { get; set; }

        public Guid DestinationWorkspaceOnDocument { get; set; }
    }
}
