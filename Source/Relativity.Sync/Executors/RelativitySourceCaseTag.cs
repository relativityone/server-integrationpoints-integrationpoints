using System;

namespace Relativity.Sync.Executors
{
    internal sealed class RelativitySourceCaseTag
    {
        public int ArtifactId { get; set; }
        
        public string Name { get; set; }

        public int SourceWorkspaceArtifactId { get; set; }

        public string SourceWorkspaceName { get; set; }

        public string SourceInstanceName { get; set; }

        public bool RequiresUpdate(string tagName, string sourceInstanceName, string sourceWorkspaceName)
        {
            return
                !string.Equals(Name, tagName, StringComparison.InvariantCulture) ||
                !string.Equals(SourceInstanceName, sourceInstanceName, StringComparison.InvariantCulture) ||
                !string.Equals(SourceWorkspaceName, sourceWorkspaceName, StringComparison.InvariantCulture);
        }
    }
}
