namespace kCura.IntegrationPoints.Data.Repositories
{
    /// <summary>
    /// Responsible for handling the Tab rdo and its functionality
    /// </summary>
    public interface ITabRepository
    {
        /// <summary>
        /// Retrieves the tab artifact id for the given object type artifact id and tab name
        /// </summary>
        /// <param name="objectTypeArtifactId">The object type artifact id</param>
        /// <param name="tabName">The name of the tab</param>
        /// <returns>The artifact id of the tab if it is found, <code>NULL</code> if not</returns>
        int? RetrieveTabArtifactId(int objectTypeArtifactId, string tabName);

        /// <summary>
        /// Deletes the given tab
        /// </summary>
        /// <param name="tabArtifactId">The artifact id of the tab to delete</param>
        void Delete(int tabArtifactId);
    }
}
