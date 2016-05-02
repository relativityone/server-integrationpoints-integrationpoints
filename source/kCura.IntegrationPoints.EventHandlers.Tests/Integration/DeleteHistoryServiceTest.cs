using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Integration
{
	[TestFixture]
	public class DeleteHistoryServiceTest : IntegrationTestBase
	{
		[Test]
		[Explicit]
		[Ignore]
		public void DeleteHistory_IntegrationPoint_success()
		{

//			IRSAPIService rsapiService = new RSAPIService(RsapiClient);
//			//rsapiService.JobHistoryLibrary = new RsapiClientLibrary<JobHistory>(client);
//
//			//rsapiService.IntegrationPointLibrary = new RsapiClientLibrary<Data.IntegrationPoint>(client);
//			var deleteHistoryError = new DeleteHistoryErrorService(rsapiService);
//			var dhs = new DeleteHistoryService(rsapiService, deleteHistoryError);
//			dhs.DeleteHistoriesAssociatedWithIP(1041683);
		}
	}
}