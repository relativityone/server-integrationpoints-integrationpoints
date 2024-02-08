using System.Collections.Generic;

namespace Relativity.IntegrationPoints.Services
{
    /// <summary>
    /// Represents a request to install source providers in a specific workspace.
    /// </summary>
    public class InstallProviderRequest
    {
        /// <summary>
        /// Workspace Id where source providers will be installed
        /// </summary>
        public int WorkspaceID { get; set; }

        /// <summary>
        /// List if soruce providers to install
        /// </summary>
        public List<InstallProviderDto> ProvidersToInstall { get; set; }
    }
}
