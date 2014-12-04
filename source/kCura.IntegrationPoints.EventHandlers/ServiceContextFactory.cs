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
		public static IServiceContext CreateServiceContext(IEHHelper helper)
		{
			return new ServiceContext
		 {
			 SqlContext = helper.GetDBContext(helper.GetActiveCaseID()),
			 RsapiService = CreateRSAPIService(helper.GetServicesManager().CreateProxy<IRSAPIClient>(ExecutionIdentity.CurrentUser)),
			 WorkspaceID = helper.GetActiveCaseID()
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
