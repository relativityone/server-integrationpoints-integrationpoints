using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.Relativity.Client;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Integration
{
	[TestFixture]
	public class IntegrationPointQueryTest
	{
		[Test]
		public void GetIntegrationPoints_SourceProviderId_IntegrationPoint()
		{
			var client = new RSAPIClient(new Uri("http://localhost/Relativity.Services"), new IntegratedAuthCredentials(), new RSAPIClientSettings())
			{
				APIOptions = { WorkspaceID = 1387214 }
			};
			IRSAPIService service = new RSAPIService();
			service.IntegrationPointLibrary = new RsapiClientLibrary<IntegrationPoint>(client);
			IntegrationPointQuery ripQuery = new IntegrationPointQuery(service);
			var value = ripQuery.GetIntegrationPoints(new List<int>() { 1039580 });


		}
	}
}
