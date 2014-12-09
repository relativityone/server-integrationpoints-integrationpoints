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
			return new ServiceContext(null)
		 {
			 SqlContext = helper.GetDBContext(workspaceID),
			 RsapiService = CreateRSAPIService(new RsapiClientFactory(helper).CreateClientForWorkspace(helper.GetActiveCaseID())),
			 WorkspaceID = workspaceID
		 };
		}

		private static IRSAPIService CreateRSAPIService(IRSAPIClient client)
		{
			return new RSAPIService()
			{
				IntegrationPointLibrary = new RsapiClientLibrary<IntegrationPoint>(client),
				SourceProviderLibrary = new RsapiClientLibrary<SourceProvider>(client)
			};
		}
	}
}
