namespace kCura.IntegrationPoints.Web.Helpers
{
    public interface IRelativityUrlHelper
    {
        /// <summary>
        /// Gets the Relativity url for a given artifact.
        /// </summary>
        /// <param name="workspaceID">The workspaces artifact id</param>
        /// <param name="artifactID">The artifact instance artifact id</param>
        /// <param name="objectTypeName">The artifact object type name</param>
        /// <returns>The url for the Relativity view as a string</returns>
        string GetRelativityViewUrl(int workspaceID, int artifactID, string objectTypeName);
    }
}
