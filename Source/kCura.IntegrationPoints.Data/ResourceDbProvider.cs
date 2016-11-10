using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
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
