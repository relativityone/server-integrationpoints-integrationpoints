using System.Security.Claims;
using kCura.IntegrationPoints.Data.Extensions;

namespace kCura.IntegrationPoints.Data
{
	public class ResourceDbProvider : IResourceDbProvider
	{
		public string GetSchemalessResourceDataBasePrepend(int workspaceId)
		{
			return ClaimsPrincipal.Current.GetSchemalessResourceDataBasePrepend(workspaceId);
		}

		public string GetResourceDbPrepend(int workspaceId)
		{
			return ClaimsPrincipal.Current.ResourceDBPrepend(workspaceId);
		}
	}
}
