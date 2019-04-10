using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoints.Core.Provider;
using kCura.IntegrationPoints.Core.Provider.Internals;
using kCura.IntegrationPoints.Data.Repositories;
using LanguageExt;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Tests.Provider
{
    [TestFixture]
    public class RipProviderUninstallerTests
    {
        private Mock<IAPILog> _loggerMock;
        private Mock<ISourceProviderRepository> _sourceProviderRepositoryMock;
        private Mock<IRelativityObjectManager> _objectManagerMock;
        private Mock<IApplicationGuidFinder> _appGuidFinderMock;
        private Mock<IDeleteIntegrationPoints> _deleteIntegrationPoints;

        private RipProviderUninstaller _sut;

        private Data.SourceProvider _sourceProviderToDelete;

        private const int _APPLICATION_ID = 454342;

        private readonly Guid _applicationGuid = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<IAPILog>();
            _sourceProviderRepositoryMock = new Mock<ISourceProviderRepository>();
            _objectManagerMock = new Mock<IRelativityObjectManager>();
            _appGuidFinderMock = new Mock<IApplicationGuidFinder>();
            _deleteIntegrationPoints = new Mock<IDeleteIntegrationPoints>();

            _sut = new RipProviderUninstaller(
                _loggerMock.Object,
                _sourceProviderRepositoryMock.Object,
                _objectManagerMock.Object,
                _appGuidFinderMock.Object,
                _deleteIntegrationPoints.Object
            );

            _appGuidFinderMock.Setup(x => x.GetApplicationGuid(It.IsAny<int>()))
                .Returns(_applicationGuid);

            _sourceProviderToDelete = new Data.SourceProvider
            {
                ArtifactId = 4232,
                Name = "ToBeDeleted"
            };

            var sourceProvidersList = new List<Data.SourceProvider> { _sourceProviderToDelete };
            _sourceProviderRepositoryMock
                .Setup(x => x.GetSourceProviderRdoByApplicationIdentifierAsync(It.IsAny<Guid>()))
                .Returns(Task.FromResult(sourceProvidersList));
        }

        [Test]
        public async Task ShouldUninstallProvider()
        {
            // act
            Either<string, Unit> result = await _sut.UninstallProvidersAsync(_APPLICATION_ID);

            // assert
            result.Should().BeRight("because provider should be uninstalled");

            _objectManagerMock.Verify(x =>
                x.Delete(
                    It.Is<Data.SourceProvider>(y => y.ArtifactId == _sourceProviderToDelete.ArtifactId),
                    It.IsAny<ExecutionIdentity>()
                )
            );
        }

        [Test]
        public async Task ShouldReturnErrorWhenCannotFindGuid()
        {
            // arrange
            string errorMessage = "Error! Cannot find guid";
            _appGuidFinderMock.Setup(x => x.GetApplicationGuid(It.IsAny<int>()))
                .Returns(errorMessage);

            // act
            Either<string, Unit> result = await _sut.UninstallProvidersAsync(_APPLICATION_ID);

            // assert
            result.Should().BeLeft(errorMessage, "because cannot find application guid for given id");
        }

        [Test]
        public async Task ShouldReturnErrorWhenExceptionWasThrownWhileGettingInstalledProviders()
        {
            // arrange
            var exceptionToThrow = new InvalidOperationException();
            _sourceProviderRepositoryMock
                .Setup(x => x.GetSourceProviderRdoByApplicationIdentifierAsync(It.IsAny<Guid>()))
                .Throws(exceptionToThrow);

            // act
            Either<string, Unit> result = await _sut.UninstallProvidersAsync(_APPLICATION_ID);

            // assert
            string expectedErrorMessage = $"Exception occured while uninstalling provider: {_APPLICATION_ID}";
            result.Should().BeLeft(expectedErrorMessage, "because cannot retrieve installed providers");
        }

        [Test]
        public async Task ShouldDeleteIntegrationPointsForSourceProvider()
        {
            // act
            await _sut.UninstallProvidersAsync(_APPLICATION_ID);

            // assert
            _deleteIntegrationPoints
                .Verify(x =>
                    x.DeleteIPsWithSourceProvider(
                        It.Is<List<int>>(list => list.Contains(_sourceProviderToDelete.ArtifactId))
                )
            );
        }
    }
}
