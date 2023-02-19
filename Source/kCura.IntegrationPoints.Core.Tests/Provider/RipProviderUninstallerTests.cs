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
    [TestFixture, Category("Unit")]
    public class RipProviderUninstallerTests
    {
        private Mock<IAPILog> _loggerMock;
        private Mock<ISourceProviderRepository> _sourceProviderRepositoryMock;
        private Mock<IApplicationGuidFinder> _appGuidFinderMock;
        private Mock<IIntegrationPointsRemover> _integrationPointRemoverMock;
        private RipProviderUninstaller _sut;
        private Data.SourceProvider _sourceProviderToDelete;
        private const int _APPLICATION_ID = 454342;
        private readonly Guid _applicationGuid = Guid.NewGuid();

        [SetUp]
        public void SetUp()
        {
            _loggerMock = new Mock<IAPILog>();
            _sourceProviderRepositoryMock = new Mock<ISourceProviderRepository>();
            _appGuidFinderMock = new Mock<IApplicationGuidFinder>();
            _integrationPointRemoverMock = new Mock<IIntegrationPointsRemover>();

            _sut = new RipProviderUninstaller(
                _loggerMock.Object,
                _sourceProviderRepositoryMock.Object,
                _appGuidFinderMock.Object,
                _integrationPointRemoverMock.Object
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
            Either<string, Unit> result = await _sut.UninstallProvidersAsync(_APPLICATION_ID).ConfigureAwait(false);

            // assert
            result.Should().BeRight("because provider should be uninstalled");

            _sourceProviderRepositoryMock.Verify(x =>
                x.Delete(
                    It.Is<Data.SourceProvider>(y => y.ArtifactId == _sourceProviderToDelete.ArtifactId)
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
            Either<string, Unit> result = await _sut.UninstallProvidersAsync(_APPLICATION_ID).ConfigureAwait(false);

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
            Either<string, Unit> result = await _sut.UninstallProvidersAsync(_APPLICATION_ID).ConfigureAwait(false);

            // assert
            string expectedErrorMessage = $"Exception occured while uninstalling provider: {_APPLICATION_ID}";
            result.Should().BeLeft(expectedErrorMessage, "because cannot retrieve installed providers");
        }

        [Test]
        public async Task ShouldDeleteIntegrationPointsForSourceProvider()
        {
            // act
            await _sut.UninstallProvidersAsync(_APPLICATION_ID).ConfigureAwait(false);

            // assert
            _integrationPointRemoverMock
                .Verify(x =>
                    x.DeleteIntegrationPointsBySourceProvider(
                        It.Is<List<int>>(list => list.Contains(_sourceProviderToDelete.ArtifactId))
                )
            );
        }
    }
}
