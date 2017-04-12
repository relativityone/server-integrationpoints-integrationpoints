using System.Security.Claims;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Data.Extensions;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;

namespace kCura.IntegrationPoints.Core.Services.Exporter
{
	public class FolderPathReaderFactory : IFolderPathReaderFactory
	{
		public IFolderPathReader Create(ClaimsPrincipal claimsPrincipal, ImportSettings importSettings, string sourceConfiguration)
		{
			if (importSettings.UseDynamicFolderPath)
			{
				var workspaceArtifactId = JsonConvert.DeserializeObject<SourceConfiguration>(sourceConfiguration).SourceWorkspaceArtifactId;
				var dbContext = claimsPrincipal.GetUnversionContext(workspaceArtifactId).ChicagoContext.DBContext;
				return new DynamicFolderPathReader(dbContext);
			}
			return new EmptyFolderPathReader();
		}
	}
}