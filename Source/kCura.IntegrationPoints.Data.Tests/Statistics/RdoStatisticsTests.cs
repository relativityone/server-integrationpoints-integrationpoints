using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Statistics.Implementations;
using kCura.Relativity.Client;
using kCura.Relativity.Client.DTOs;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Data.Tests.Statistics
{
	[TestFixture]
	public class RdoStatisticsTests : TestBase
	{
		private const int _WORKSPACE_ID = 246693;

		private IRdoRepository _rdoRepository;
		private RdoStatistics _instance;

		public override void SetUp()
		{
			_rdoRepository = Substitute.For<IRdoRepository>();

			var repositoryFactory = Substitute.For<IRepositoryFactory>();
			repositoryFactory.GetRdoRepository(_WORKSPACE_ID).Returns(_rdoRepository);

			_instance = new RdoStatistics(repositoryFactory);
		}

		[Test]
		public void ItShouldReturnRdoTotalCount()
		{
			// ARRANGE
			int expectedResult = 333;

			int artifactTypeId = 912651;
			int viewId = 391152;

			_rdoRepository.Query(Arg.Is<Query<RDO>>(x => x.ArtifactTypeID == artifactTypeId && x.Condition is ViewCondition)).Returns(new QueryResultSet<RDO>
			{
				TotalCount = expectedResult
			});

			// ACT
			var actualResult = _instance.ForView(_WORKSPACE_ID, artifactTypeId, viewId);

			// ASSERT
			Assert.That(actualResult, Is.EqualTo(expectedResult));

			_rdoRepository.Received(1).Query(Arg.Is<Query<RDO>>(x => x.ArtifactTypeID == artifactTypeId && x.Condition is ViewCondition));
		}
	}
}