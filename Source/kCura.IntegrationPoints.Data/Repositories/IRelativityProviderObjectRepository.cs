namespace kCura.IntegrationPoints.Data.Repositories
{
    public interface IRelativityProviderObjectRepository
    {
        /// <summary>
        ///     Creates the object type in the given workspace
        /// </summary>
        /// <param name="parentArtifactTypeId">Parent Artifact Type id</param>
        /// <returns>The artifact type id of the newly created object</returns>
        int CreateObjectType(int parentArtifactTypeId);

        /// <summary>
        ///     Creates the Source Workspace field on the Document object
        /// </summary>
        /// <param name="sourceWorkspaceObjectTypeId">The Source Workspace artifact type id</param>
        /// <returns>The artifact id of the newly created field</returns>
        int CreateFieldOnDocument(int sourceWorkspaceObjectTypeId);
    }
}