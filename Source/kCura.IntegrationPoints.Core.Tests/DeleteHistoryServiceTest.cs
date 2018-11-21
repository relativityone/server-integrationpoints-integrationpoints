using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Tests
{
	[TestFixture]
	public class DeleteHistoryServiceTest : TestBase
	{
		private DeleteHistoryService _instance;
		private IRelativityObjectManager _objectManager;

		private const int _WORKSPACE_ID = 813386;
		
		[SetUp]
		public override void SetUp()
		{
			_objectManager = Substitute.For<IRelativityObjectManager>();

			IRelativityObjectManagerFactory rsapiServiceFactory = Substitute.For<IRelativityObjectManagerFactory>();
			rsapiServiceFactory.CreateRelativityObjectManager(_WORKSPACE_ID).Returns(_objectManager);

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

			_objectManager.Query<Data.IntegrationPoint>(Arg.Any<QueryRequest>()).Returns(integrationPoint);

			//ACT
			_instance.DeleteHistoriesAssociatedWithIPs(integrationPointsId, _objectManager);

			//ASSERT
			_objectManager.Received(2).Update(Arg.Is<Data.IntegrationPoint>( x => !x.JobHistory.Any()));
		}
	}
}