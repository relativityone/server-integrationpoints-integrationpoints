using kCura.IntegrationPoint.Tests.Core;
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
		private IRdoRepository _rdoRepository;
		private RdoStatistics _instance;

		public override void SetUp()
		{
			_rdoRepository = Substitute.For<IRdoRepository>();

			_instance = new RdoStatistics(_rdoRepository);
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
			var actualResult = _instance.ForView(artifactTypeId, viewId);

			// ASSERT
			Assert.That(actualResult, Is.EqualTo(expectedResult));

			_rdoRepository.Received(1).Query(Arg.Is<Query<RDO>>(x => x.ArtifactTypeID == artifactTypeId && x.Condition is ViewCondition));
		}
	}
}