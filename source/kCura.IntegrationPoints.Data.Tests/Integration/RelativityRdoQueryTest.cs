﻿using kCura.IntegrationPoint.Tests.Core;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Integration
{
	[TestFixture]
	public class RelativityRdoQueryTest : IntegrationTestBase
	{
		[Test]
		[Explicit]
		public void RelativityRdoQueryReturnsAllRdo()
		{   //ARRANGE
			var rdoQuery = new RSAPIRdoQuery(GerronHelper.Rsapi.CreateRsapiClient());
			var data = rdoQuery.GetAllRdo();

			Assert.IsNotNull(data);
		}
	}
}