using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.EventHandlers
{
	public class ServiceContextFactory
	{
		public static IServiceContext CreateServiceContext(IEHHelper helper, int workspaceID)
		{
			return new ServiceContext
		 {
			 SqlContext = helper.GetDBContext(workspaceID),
			 RsapiService = CreateRSAPIService(helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser)),
			 WorkspaceID = workspaceID
		 };
		}

		private static IRSAPIService CreateRSAPIService(IRSAPIClient client)
		{
			return new RSAPIService
			{
				IntegrationPointLibrary = new RsapiClientLibrary<IntegrationPoint>(client),
				SourceProviderLibrary = new RsapiClientLibrary<SourceProvider>(client)
			};
		}
	}
}
