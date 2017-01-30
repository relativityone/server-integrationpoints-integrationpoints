using System.Security.Claims;
using kCura.IntegrationPoints.Data.Extensions;
using Relativity.Core;
using Relativity.SecretCatalog;

namespace kCura.IntegrationPoints.Data.SecretStore
{
	public class DefaultSecretCatalogFactory : ISecretCatalogFactory
	{
		public ISecretCatalog Create(int workspaceArtifactId)
		{
			var baseContext = ClaimsPrincipal.Current.GetUnversionContext(workspaceArtifactId).GetMasterRdgContext();
			return SecretStoreFactory.GetSecretStore(baseContext);
		}
	}
}