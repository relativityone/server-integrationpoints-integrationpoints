using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Integration
{
	[TestFixture]
	class DeleteIPsWithSourceProviderTest
	{

		[Test]
		[Explicit]
		public void DeleteIPsWithSourceProvider_id_points()
		{
			var client = new RSAPIClient(new Uri("http://localhost/Relativity.Services"), new IntegratedAuthCredentials(), new RSAPIClientSettings())
			{
				APIOptions = { WorkspaceID = 1390997 }
			};
			IRSAPIService service = new RSAPIService();
			DeleteHistoryErrorService deleteerErrorService = new DeleteHistoryErrorService(service);
			service.IntegrationPointLibrary = new RsapiClientLibrary<IntegrationPoint>(client);
			service.JobHistoryLibrary = new RsapiClientLibrary<JobHistory>(client);
			service.SourceProviderLibrary = new RsapiClientLibrary<SourceProvider>(client);
			IntegrationPointQuery integrationPointQuery = new IntegrationPointQuery(service);
			DeleteHistoryService deleteHistoryService = new DeleteHistoryService(service,deleteerErrorService);

			var deletePoints = new DeleteIntegrationPoints(integrationPointQuery, deleteHistoryService, service);
			deletePoints.DeleteIPsWithSourceProvider(new List<int>() { 1040675 });
		}
	}
}
