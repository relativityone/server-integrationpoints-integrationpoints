using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.Relativity.Client;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Integration
{
	[TestFixture]
	public class DeleteHistoryServiceTest
	{
		[Test]
		[Explicit]
		public void DeleteHistory_IntegrationPoint_success()
		{
			var client = new RSAPIClient(new Uri("http://localhost/Relativity.Services"), new IntegratedAuthCredentials(), new RSAPIClientSettings())
			{
				APIOptions = { WorkspaceID = 1391094 }
			};

			IRSAPIService rsapiService = new RSAPIService(client);
			//rsapiService.JobHistoryLibrary = new RsapiClientLibrary<JobHistory>(client);

			//rsapiService.IntegrationPointLibrary = new RsapiClientLibrary<Data.IntegrationPoint>(client);
			var deleteHistoryError = new DeleteHistoryErrorService(rsapiService);
			var dhs = new DeleteHistoryService(rsapiService,deleteHistoryError);
			dhs.DeleteHistoriesAssociatedWithIP(1041683);

		}
	}
}
