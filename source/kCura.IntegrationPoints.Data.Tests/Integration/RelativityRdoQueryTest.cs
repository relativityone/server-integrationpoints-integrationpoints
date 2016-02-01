using System;
using kCura.Relativity.Client;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Integration
{
	[TestFixture]
	public class RelativityRdoQueryTest
	{
		[Test]
		[Explicit]
		public void RelativityRdoQueryReturnsAllRdo()
		{	//ARRANGE
			var client = new RSAPIClient(new Uri("http://localhost/Relativity.Services"), new IntegratedAuthCredentials())
			{
				APIOptions = { WorkspaceID = 1383218 }
			};
			var rdoQuery = new RSAPIRdoQuery(client);

			var data = rdoQuery.GetAllRdo();
		}


	}
}
