using System.Security.Claims;
using kCura.IntegrationPoints.Synchronizers.RDO;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public interface IFolderPathReaderFactory
	{
		IFolderPathReader Create(ClaimsPrincipal claimsPrincipal, ImportSettings importSettings, string sourceConfiguration);
	}
}