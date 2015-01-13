﻿using System.Collections.Generic;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.ServiceContext
{
	public class ServiceContextFactory
	{
		public static ICaseServiceContext CreateCaseServiceContext(IEHHelper helper, int workspaceID)
		{
			return new CaseServiceContext(new ServiceContextHelperForEventHandlers(helper, workspaceID, new RsapiClientFactory(helper)));
		}

		public static IRSAPIService CreateRSAPIService(IRSAPIClient client)
		{
			return new RSAPIService()
			{
				IntegrationPointLibrary = new RsapiClientLibrary<IntegrationPoint>(client),
				SourceProviderLibrary = new RsapiClientLibrary<SourceProvider>(client)
			};
		}
	}
}
