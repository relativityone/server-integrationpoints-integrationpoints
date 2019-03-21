using System.Collections.Generic;

namespace kCura.IntegrationPoints.Services
{
	public class InstallProviderRequest
	{
		/// <summary>
		/// TODO
		/// </summary>
		public int WorkspaceID { get; set; }

		/// <summary>
		/// TODO
		/// </summary>
		public List<ProviderToInstallDto> ProvidersToInstall { get; set; }
	}
}
