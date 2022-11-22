using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Provider.Internals;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;

namespace kCura.IntegrationPoints.Core.Tests.Provider.Internals
{
    [TestFixture, Category("Unit")]
    public class IntegrationPointsRemoverTests : TestBase
    {
        private IDeleteHistoryService _deleteHistoryService;
        private IIntegrationPointRepository _integrationPointRepository;

        [SetUp]
        public override void SetUp()
        {
            _integrationPointRepository = Substitute.For<IIntegrationPointRepository>();
            _integrationPointRepository.GetIntegrationPoints(Arg.Any<List<int>>()).Returns(new List<Data.IntegrationPoint>());
            _deleteHistoryService = Substitute.For<IDeleteHistoryService>();
        }

        [Test]
        public void ShouldThrowExceptionWhenDeleteHistoryServiceIsNullTest()
        {
            // arrange
            var sut = new IntegrationPointsRemover(null, _integrationPointRepository);
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
            var sut = new IntegrationPointsRemover(_deleteHistoryService, _integrationPointRepository);

            // act
            sut.DeleteIntegrationPointsBySourceProvider(sourceProvider.ToList());

            // assert
            _integrationPointRepository.Received(1).GetIntegrationPoints(Arg.Is<List<int>>(x => x.SequenceEqual(sourceProvider)));
        }

        [TestCase(new int[0], new int[0])]
        [TestCase(new[] { 1, 2, 3 }, new[] { 5, 7, 6 })]
        public void ShouldCallWithProperArgumentsDeleteHistoryServiceTest(int[] sourceProvider, int[] artifactIds)
        {
            // arrange
            List<Data.IntegrationPoint> integrationPoints = GetMockIntegrationsPoints(artifactIds).ToList();
            _integrationPointRepository.GetIntegrationPoints(Arg.Any<List<int>>()).Returns(integrationPoints);
            var sut = new IntegrationPointsRemover(_deleteHistoryService, _integrationPointRepository);

            // act
            sut.DeleteIntegrationPointsBySourceProvider(sourceProvider.ToList());

            // assert
            _deleteHistoryService.Received(1).DeleteHistoriesAssociatedWithIPs(Arg.Is<List<int>>(x => x.SequenceEqual(artifactIds)));
        }

        [TestCase(new int[0], new int[0])]
        [TestCase(new[] { 1, 2, 3 }, new[] { 5, 7, 6 })]
        public void ShouldCallWithProperArgumentsIntegrationPointLibraryTest(int[] sourceProvider, int[] artifactIds)
        {
            // arrange
            List<Data.IntegrationPoint> integrationPoints = GetMockIntegrationsPoints(artifactIds).ToList();
            _integrationPointRepository.GetIntegrationPoints(Arg.Any<List<int>>()).Returns(integrationPoints);
            var sut = new IntegrationPointsRemover(_deleteHistoryService, _integrationPointRepository);

            // act
            sut.DeleteIntegrationPointsBySourceProvider(sourceProvider.ToList());

            // assert
            foreach (Data.IntegrationPoint ip in integrationPoints)
            {
                _integrationPointRepository.Received(1)
                    .Delete(Arg.Is(ip.ArtifactId));
            }
        }

        [Test]
        public void ShouldCallWithProperArgumentsIntegrationPointLibraryWhenCalledTwoTimesTest()
        {
            // arrange
            var sourceProvider = new List<int> { 1 };
            var sut = new IntegrationPointsRemover(_deleteHistoryService, _integrationPointRepository);

            int[] artifactIds = { 7 };
            List<Data.IntegrationPoint> integrationPoints = GetMockIntegrationsPoints(artifactIds).ToList();
            _integrationPointRepository.GetIntegrationPoints(Arg.Any<List<int>>()).Returns(integrationPoints);

            sut.DeleteIntegrationPointsBySourceProvider(sourceProvider);

            int[] secondArtifactIds = { 8 };
            List<Data.IntegrationPoint> secondintegrationPoints = GetMockIntegrationsPoints(secondArtifactIds).ToList();
            _integrationPointRepository.GetIntegrationPoints(Arg.Any<List<int>>()).Returns(secondintegrationPoints);

            // act
            sut.DeleteIntegrationPointsBySourceProvider(sourceProvider);

            // assert
            foreach (Data.IntegrationPoint ip in integrationPoints)
            {
                _integrationPointRepository.Received(1).Delete(Arg.Is(ip.ArtifactId));
            }
            foreach (Data.IntegrationPoint ip in secondintegrationPoints)
            {
                _integrationPointRepository.Received(1).Delete(Arg.Is(ip.ArtifactId));
            }
        }

        private IEnumerable<Data.IntegrationPoint> GetMockIntegrationsPoints(IEnumerable<int> artifactIds)
        {
            return artifactIds.Select(artifactId => new Data.IntegrationPoint { ArtifactId = artifactId });
        }
    }
}
