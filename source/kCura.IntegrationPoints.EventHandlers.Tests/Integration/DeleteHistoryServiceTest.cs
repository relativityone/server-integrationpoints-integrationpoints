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
		public void CheckIfItWorksDeleteLater()
		{
			var client = new RSAPIClient(new Uri("http://localhost/Relativity.Services"), new IntegratedAuthCredentials(), new RSAPIClientSettings())
			{
				APIOptions = { WorkspaceID = 1387214 }
			};
			IRSAPIService rsapiService = new RSAPIService();
			rsapiService.JobHistoryLibrary = new RsapiClientLibrary<JobHistory>(client);
			rsapiService.IntegrationPointLibrary = new RsapiClientLibrary<Data.IntegrationPoint>(client);
			DeleteHistoryService dhs = new DeleteHistoryService(rsapiService);
			dhs.DeleteHistoriesAssociatedWithIP(1039602);

		}
	}
}
