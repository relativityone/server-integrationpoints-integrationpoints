using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Statistics.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Tests.Statistics
{
    [TestFixture, Category("Unit")]
    public class RdoStatisticsTests : TestBase
    {
        private IRelativityObjectManager _relativityObjectManager;
        private RdoStatistics _instance;

        public override void SetUp()
        {
            _relativityObjectManager = Substitute.For<IRelativityObjectManager>();

            _instance = new RdoStatistics(_relativityObjectManager);
        }

        [Test]
        public void ItShouldReturnRdoTotalCount()
        {
            // ARRANGE
            int expectedResult = 333;

            int artifactTypeId = 912651;
            int viewId = 391152;

            _relativityObjectManager.QueryTotalCount(Arg.Is<QueryRequest>(x => x.ObjectType.ArtifactTypeID == artifactTypeId)).Returns(expectedResult);

            // ACT
            var actualResult = _instance.ForView(artifactTypeId, viewId);

            // ASSERT
            Assert.That(actualResult, Is.EqualTo(expectedResult));

            _relativityObjectManager.Received(1).QueryTotalCount(Arg.Is<QueryRequest>(x => x.ObjectType.ArtifactTypeID == artifactTypeId));
        }
    }
}