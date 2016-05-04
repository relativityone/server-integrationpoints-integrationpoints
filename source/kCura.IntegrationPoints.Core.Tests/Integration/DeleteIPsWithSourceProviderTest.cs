using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Integration
{
	[TestFixture]
	internal class DeleteIPsWithSourceProviderTest : IntegrationTestBase
	{
		[Test]
		[Explicit]
		[Ignore]
		public void DeleteIPsWithSourceProvider_id_points()
		{
//			IRSAPIService service = new RSAPIService();
//			DeleteHistoryErrorService deleteerErrorService = new DeleteHistoryErrorService(service);
//			service.IntegrationPointLibrary = new RsapiClientLibrary<Data.IntegrationPoint>(RsapiClient);
//			service.JobHistoryLibrary = new RsapiClientLibrary<JobHistory>(RsapiClient);
//			service.SourceProviderLibrary = new RsapiClientLibrary<SourceProvider>(RsapiClient);
//			IntegrationPointQuery integrationPointQuery = new IntegrationPointQuery(service);
//			DeleteHistoryService deleteHistoryService = new DeleteHistoryService(service, deleteerErrorService);
//
//			var deletePoints = new DeleteIntegrationPoints(integrationPointQuery, deleteHistoryService, service);
//			deletePoints.DeleteIPsWithSourceProvider(new List<int>() { 1040675 });
		}
	}
}