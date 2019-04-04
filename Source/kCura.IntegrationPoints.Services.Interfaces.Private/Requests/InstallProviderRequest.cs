using System.Collections.Generic;

namespace kCura.IntegrationPoints.Services
{
	public class InstallProviderRequest
	{
		/// <summary>
		/// Workspace Id where source providers will be installed
		/// </summary>
		public int WorkspaceID { get; set; }

		/// <summary>
		/// List if soruce providers to install
		/// </summary>
		public List<ProviderToInstallDto> ProvidersToInstall { get; set; }
	}
}
