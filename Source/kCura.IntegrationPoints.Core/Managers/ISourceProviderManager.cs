namespace kCura.IntegrationPoints.Core.Managers
{
    public interface ISourceProviderManager
    {
        /// <summary>
        /// Gets the Source Provider artifact id given a guid identifier
        /// </summary>
        /// <param name="workspaceArtifactId">Workspace id of the integration point instance</param>
        /// <param name="sourceProviderGuidIdentifier">Guid identifier of Source Provider type</param>
        /// <returns>Artifact id of the Source Provider</returns>
        int GetArtifactIdFromSourceProviderTypeGuidIdentifier(int workspaceArtifactId, string sourceProviderGuidIdentifier);
    }
}
