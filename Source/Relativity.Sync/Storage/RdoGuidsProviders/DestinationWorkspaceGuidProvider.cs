using System;
using Relativity.Sync.Configuration;

namespace Relativity.Sync.Storage.RdoGuidsProviders
{
    internal class DestinationWorkspaceTagGuidProvider : IDestinationWorkspaceTagGuidProvider
    {
        private readonly IConfiguration _configuration;

        public DestinationWorkspaceTagGuidProvider(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public Guid TypeGuid => _configuration.GetFieldValue(x => x.DestinationWorkspaceType);

        public Guid NameGuid => _configuration.GetFieldValue(x => x.DestinationWorkspaceNameField);

        public Guid DestinationWorkspaceNameGuid =>
            _configuration.GetFieldValue(x => x.DestinationWorkspaceDestinationWorkspaceName);

        public Guid DestinationWorkspaceArtifactIdGuid =>
            _configuration.GetFieldValue(x => x.DestinationWorkspaceWorkspaceArtifactIdField);

        public Guid DestinationInstanceNameGuid =>
            _configuration.GetFieldValue(x => x.DestinationWorkspaceDestinationInstanceName);

        public Guid DestinationInstanceArtifactIdGuid =>
            _configuration.GetFieldValue(x => x.DestinationWorkspaceDestinationInstanceArtifactId);


        public Guid JobHistoryOnDocumentGuid => _configuration.GetFieldValue(x => x.JobHistoryOnDocumentField);

        public Guid DestinationWorkspaceOnDocument =>
            _configuration.GetFieldValue(x => x.DestinationWorkspaceOnDocumentField);
    }
}
