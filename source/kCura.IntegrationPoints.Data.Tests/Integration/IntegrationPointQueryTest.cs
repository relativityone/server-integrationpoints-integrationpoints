using System;
using System.Collections.Generic;
using kCura.Relativity.Client;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Integration
{
	[TestFixture]
	public class IntegrationPointQueryTest : IntegrationTestBase
	{
		[Explicit]
		[Test]
		public void GetIntegrationPoints_SourceProviderId_IntegrationPoint()
		{
			IRSAPIService service = new RSAPIService();
			service.IntegrationPointLibrary = new RsapiClientLibrary<IntegrationPoint>(RsapiClient);
			IntegrationPointQuery ripQuery = new IntegrationPointQuery(service);
			var value = ripQuery.GetIntegrationPoints(new List<int>() { 1039580 });
		}
	}
}
