using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Provider.Internals;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Core.Tests.Provider.Internals
{
    [TestFixture]
    public class IntegrationPointsRemoverTests : TestBase
    {
        private IIntegrationPointQuery _integrationPointQuery;
        private IDeleteHistoryService _deleteHistoryService;
        private IRelativityObjectManager _relativityObjectManager;

        [SetUp]
        public override void SetUp()
        {
            _relativityObjectManager = Substitute.For<IRelativityObjectManager>();

            _deleteHistoryService = Substitute.For<IDeleteHistoryService>();
            _integrationPointQuery = Substitute.For<IIntegrationPointQuery>();
        }

        [Test]
        public void ShouldThrowExceptionWhenIntegrationPointQueryIsNullTest()
        {
            // arrange
            var sut = new IntegrationPointsRemover(null, _deleteHistoryService, _relativityObjectManager);
            var ids = new List<int> { 1, 2, 3 };

            // act
            Action deleteAction = () => sut.DeleteIntegrationPointsBySourceProvider(ids);

            // assert
            deleteAction.ShouldThrow<NullReferenceException>();
        }

        [Test]
        public void ShouldThrowExceptionWhenDeleteHistoryServiceIsNullTest()
        {
            // arrange
            var sut = new IntegrationPointsRemover(_integrationPointQuery, null, _relativityObjectManager);
            var ids = new List<int> { 1, 2, 3 };

            // act
            Action deleteAction = () => sut.DeleteIntegrationPointsBySourceProvider(ids);

            // assert
            deleteAction.ShouldThrow<NullReferenceException>();
        }

        [TestCase(new int[] { })]
        [TestCase(new[] { 1 })]
        [TestCase(new[] { 1, 6, 3 })]
        public void ShouldCallWithProperArgumentIntegrationPointQueryTest(int[] sourceProvider)
        {
            // arrange
            var sut = new IntegrationPointsRemover(_integrationPointQuery, _deleteHistoryService, _relativityObjectManager);

            // act
            sut.DeleteIntegrationPointsBySourceProvider(sourceProvider.ToList());

            // assert
            _integrationPointQuery.Received(1).GetIntegrationPoints(Arg.Is<List<int>>(x => x.SequenceEqual(sourceProvider)));
        }

        [TestCase(new int[0], new int[0])]
        [TestCase(new[] { 1, 2, 3 }, new[] { 5, 7, 6 })]
        public void ShouldCallWithProperArgumentsDeleteHistoryServiceTest(int[] sourceProvider, int[] artifactIds)
        {
            // arrange
            List<Data.IntegrationPoint> integrationPoints = GetMockIntegrationsPoints(artifactIds).ToList();
            _integrationPointQuery.GetIntegrationPoints(Arg.Any<List<int>>()).Returns(integrationPoints);
            var sut = new IntegrationPointsRemover(_integrationPointQuery, _deleteHistoryService, _relativityObjectManager);

            // act
            sut.DeleteIntegrationPointsBySourceProvider(sourceProvider.ToList());

            // assert
            _deleteHistoryService.Received(1).DeleteHistoriesAssociatedWithIPs(Arg.Is<List<int>>(x => x.SequenceEqual(artifactIds)), _relativityObjectManager);
        }

        [TestCase(new int[0], new int[0])]
        [TestCase(new[] { 1, 2, 3 }, new[] { 5, 7, 6 })]
        public void ShouldCallWithProperArgumentsIntegrationPointLibraryTest(int[] sourceProvider, int[] artifactIds)
        {
            // arrange
            List<Data.IntegrationPoint> integrationPoints = GetMockIntegrationsPoints(artifactIds).ToList();
            _integrationPointQuery.GetIntegrationPoints(Arg.Any<List<int>>()).Returns(integrationPoints);
            var sut = new IntegrationPointsRemover(_integrationPointQuery, _deleteHistoryService, _relativityObjectManager);

            // act
            sut.DeleteIntegrationPointsBySourceProvider(sourceProvider.ToList());

            // assert
            foreach (Data.IntegrationPoint ip in integrationPoints)
            {
                _relativityObjectManager.Received(1)
                    .Delete(Arg.Is<Data.IntegrationPoint>(x => x.ArtifactId == ip.ArtifactId));
            }
        }

        [Test]
        public void ShouldCallWithProperArgumentsIntegrationPointLibraryWhenCalledTwoTimesTest()
        {
            // arrange
            var sourceProvider = new List<int> { 1 };
            var sut = new IntegrationPointsRemover(_integrationPointQuery, _deleteHistoryService, _relativityObjectManager);

            int[] artifactIds = { 7 };
            List<Data.IntegrationPoint> integrationPoints = GetMockIntegrationsPoints(artifactIds).ToList();
            _integrationPointQuery.GetIntegrationPoints(Arg.Any<List<int>>()).Returns(integrationPoints);

            sut.DeleteIntegrationPointsBySourceProvider(sourceProvider);

            int[] secondArtifactIds = { 8 };
            List<Data.IntegrationPoint> secondintegrationPoints = GetMockIntegrationsPoints(secondArtifactIds).ToList();
            _integrationPointQuery.GetIntegrationPoints(Arg.Any<List<int>>()).Returns(secondintegrationPoints);

            // act
            sut.DeleteIntegrationPointsBySourceProvider(sourceProvider);

            // assert
            foreach (Data.IntegrationPoint ip in integrationPoints)
            {
                _relativityObjectManager.Received(1).Delete(Arg.Is<Data.IntegrationPoint>(x => x.ArtifactId == ip.ArtifactId));
            }
            foreach (Data.IntegrationPoint ip in secondintegrationPoints)
            {
                _relativityObjectManager.Received(1).Delete(Arg.Is<Data.IntegrationPoint>(x => x.ArtifactId == ip.ArtifactId));
            }
        }

        private IEnumerable<Data.IntegrationPoint> GetMockIntegrationsPoints(IEnumerable<int> artifactIds)
        {
            return artifactIds.Select(artifactId => new Data.IntegrationPoint { ArtifactId = artifactId });
        }
    }
}
