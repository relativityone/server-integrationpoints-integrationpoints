using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Data.Repositories;
using NSubstitute;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services.Repositories.Implementations;
using Relativity.Services.Objects.DataContracts;

namespace Relativity.IntegrationPoints.Services.Tests.Repositories
{
    [TestFixture, Category("Unit")]
    public class ProviderRepositoryTests : TestBase
    {
        private ProviderAccessor _providerAccessor;
        private IRepositoryFactory _repositoryFactory;
        private IRelativityObjectManagerService _relativityObjectManagerService;

        public override void SetUp()
        {
            _repositoryFactory = Substitute.For<IRepositoryFactory>();
            _relativityObjectManagerService = Substitute.For<IRelativityObjectManagerService>();

            _providerAccessor = new ProviderAccessor(_repositoryFactory, _relativityObjectManagerService);
        }

        [Test]
        public void ItShouldGetSourceProviderArtifactId()
        {
            int workspaceId = 782;
            string guid = "guid_905";

            int expectedSourceProviderArtifactId = 889;

            var sourceProviderRepository = Substitute.For<ISourceProviderRepository>();
            _repositoryFactory.GetSourceProviderRepository(workspaceId).Returns(sourceProviderRepository);

            sourceProviderRepository.GetArtifactIdFromSourceProviderTypeGuidIdentifier(guid).Returns(expectedSourceProviderArtifactId);

            var actualResult = _providerAccessor.GetSourceProviderArtifactId(workspaceId, guid);

            Assert.That(actualResult, Is.EqualTo(expectedSourceProviderArtifactId));
        }

        [Test]
        public void ItShouldGetDestinationProviderArtifactId()
        {
            int workspaceId = 329;
            string guid = "guid_819";

            int expectedDestinationProviderArtifactId = 889;

            var destinationProviderRepository = Substitute.For<IDestinationProviderRepository>();
            _repositoryFactory.GetDestinationProviderRepository(workspaceId).Returns(destinationProviderRepository);

            destinationProviderRepository.GetArtifactIdFromDestinationProviderTypeGuidIdentifier(guid).Returns(expectedDestinationProviderArtifactId);

            var actualResult = _providerAccessor.GetDestinationProviderArtifactId(workspaceId, guid);

            Assert.That(actualResult, Is.EqualTo(expectedDestinationProviderArtifactId));
        }

        [Test]
        public void ItShouldGetAllSourceProviders()
        {
            var objectManager = Substitute.For<IRelativityObjectManager>();
            _relativityObjectManagerService.RelativityObjectManager.Returns(objectManager);

            var expectedResult = new List<SourceProvider>
            {
                new SourceProvider
                {
                    ArtifactId = 369,
                    Name = "name_572"
                },
                new SourceProvider
                {
                    ArtifactId = 229,
                    Name = "name_700"
                }
            };

            objectManager.Query<SourceProvider>(Arg.Any<QueryRequest>()).Returns(expectedResult);

            var actualResult = _providerAccessor.GetSourceProviders(521);

            Assert.That(actualResult,
                Is.EquivalentTo(expectedResult).Using(new Func<ProviderModel, SourceProvider, bool>((x, y) => (x.Name == y.Name) && (x.ArtifactId == y.ArtifactId))));
        }

        [Test]
        public void ItShouldGetAllDestinationProviders()
        {
            var objectManager = Substitute.For<IRelativityObjectManager>();
            _relativityObjectManagerService.RelativityObjectManager.Returns(objectManager);

            var expectedResult = new List<DestinationProvider>
            {
                new DestinationProvider
                {
                    ArtifactId = 281,
                    Name = "name_818"
                },
                new DestinationProvider
                {
                    ArtifactId = 918,
                    Name = "name_423"
                }
            };

            objectManager.Query<DestinationProvider>(Arg.Any<QueryRequest>()).Returns(expectedResult);

            var actualResult = _providerAccessor.GetDesinationProviders(521);

            Assert.That(actualResult,
                Is.EquivalentTo(expectedResult).Using(new Func<ProviderModel, DestinationProvider, bool>((x, y) => (x.Name == y.Name) && (x.ArtifactId == y.ArtifactId))));
        }
    }
}
