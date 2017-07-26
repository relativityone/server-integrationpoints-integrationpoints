using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests
{
	[TestFixture]
	public class DeleteHistoryServiceTest : TestBase
	{
		private const int _WORKSPACE_ID = 813386;

		private DeleteHistoryService _instance;
		private IRSAPIService _rsapiService;

		[SetUp]
		public override void SetUp()
		{
			_rsapiService = Substitute.For<IRSAPIService>();

			IRSAPIServiceFactory rsapiServiceFactory = Substitute.For<IRSAPIServiceFactory>();
			rsapiServiceFactory.Create(_WORKSPACE_ID).Returns(_rsapiService);

			_instance = new DeleteHistoryService(rsapiServiceFactory);
		}

		[Test]
		public void ItShouldSetJobHistoryNull()
		{
			//ARRANGE
			var integrationPointsId = new List<int> {1, 2};

			var integrationPoint = new List<Data.IntegrationPoint>
			{
				new Data.IntegrationPoint
				{
					JobHistory = new[] {1, 2, 3}
				},
				new Data.IntegrationPoint
				{
					JobHistory = new[] {1, 2, 3}
				}
			};

			_rsapiService.IntegrationPointLibrary.Query(Arg.Any<Query<RDO>>()).Returns(integrationPoint);

			//ACT
			_instance.DeleteHistoriesAssociatedWithIPs(integrationPointsId, _rsapiService);

			//ASSERT
			_rsapiService.IntegrationPointLibrary.Received().Update(Arg.Is<IEnumerable<Data.IntegrationPoint>>(x => x.All(y => !y.JobHistory.Any())));
		}
	}
}