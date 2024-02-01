namespace Relativity.IntegrationPoints.Services
{
    /// <summary>
    /// Represents a request to uninstall a source provider from a specific workspace.
    /// </summary>
    public class UninstallProviderRequest
    {
        /// <summary>
        /// Workspace Id where source providers will be uninstalled
        /// </summary>
        public int WorkspaceID { get; set; }

        /// <summary>
        /// Id of source provider to uninstall
        /// </summary>
        public int ApplicationID { get; set; }
    }
}
