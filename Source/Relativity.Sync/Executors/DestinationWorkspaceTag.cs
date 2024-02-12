using System;

namespace Relativity.Sync.Executors
{
    internal sealed class DestinationWorkspaceTag
    {
        public int ArtifactId { get; set; }

        public string DestinationWorkspaceName { get; set; }

        public string DestinationInstanceName { get; set; }

        public int DestinationWorkspaceArtifactId { get; set; }

        public bool RequiresUpdate(string destinationWorkspaceName, string destinationInstanceName)
        {
            return !string.Equals(destinationWorkspaceName, DestinationWorkspaceName, StringComparison.InvariantCulture) ||
                !string.Equals(destinationInstanceName, DestinationInstanceName, StringComparison.InvariantCulture);
        }
    }
}
