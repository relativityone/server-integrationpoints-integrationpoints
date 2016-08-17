using kCura.IntegrationPoint.Tests.Core;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Integration
{
	[TestFixture]
	[Category(kCura.IntegrationPoint.Tests.Core.Constants.INTEGRATION_CATEGORY)]
	public class RelativityRdoQueryTest : IntegrationTestBase
	{
		[Test]
		[Explicit]
		public void RelativityRdoQueryReturnsAllRdo()
		{   //ARRANGE
			var rdoQuery = new RSAPIRdoQuery(Rsapi.CreateRsapiClient());
			var data = rdoQuery.GetAllRdo();

			Assert.IsNotNull(data);
		}
	}
}