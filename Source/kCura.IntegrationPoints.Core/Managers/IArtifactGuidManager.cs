using System;
using System.Collections.Generic;

namespace kCura.IntegrationPoints.Core.Managers
{
    public interface IArtifactGuidManager
    {
        /// <summary>
        /// Gets GUIDs for Artifact Ids
        /// </summary>
        /// <param name="workspaceArtifactId">Workspace artifact id</param>
        /// <param name="artifactIds">Artifact Ids</param>
        /// <returns>GUID value</returns>
        Dictionary<int, Guid> GetGuidsForArtifactIds(int workspaceArtifactId, IEnumerable<int> artifactIds);

        /// <summary>
        /// Gets Artifact Ids for GUIDs
        /// </summary>
        /// <param name="workspaceArtifactId">Workspace artifact id</param>
        /// <param name="guids">GUIDs</param>
        /// <returns>Dictionary of GUIDs and Artifact Ids</returns>
        Dictionary<Guid, int> GetArtifactIdsForGuids(int workspaceArtifactId, IEnumerable<Guid> guids);
    }
}
