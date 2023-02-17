using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core.FluentAssertions;
using kCura.IntegrationPoints.Core.Provider;
using kCura.IntegrationPoints.Core.Provider.Internals;
using kCura.IntegrationPoints.Core.Services.Domain;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Data.Repositories;
using LanguageExt;
using Moq;
using Moq.Language.Flow;
using NUnit.Framework;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Contracts.Internals;
using Relativity.IntegrationPoints.Contracts.Provider;
using static LanguageExt.Prelude;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using Relativity.Toggles;

namespace kCura.IntegrationPoints.Core.Tests.Provider
{
    [TestFixture, Category("Unit")]
    public class RipProviderInstallerTests
    {
        private Mock<ISourceProviderRepository> _sourceProviderRepositoryMock;
        private Mock<IApplicationGuidFinder> _appGuidFinderMock;
        private Mock<IDataProviderFactoryFactory> _dataProviderFactoryFactoryMock;
        private Mock<IAPILog> _loggerFake;
        private Mock<IToggleProvider> _toggleProvider;
        private RipProviderInstaller _sut;
        private SourceProvider _sourceProviderToCreate;
        private SourceProvider[] _sourceProvidersToCreate;
        private Guid _existingProviderApplicationGuid;

        [SetUp]
        public void SetUp()
        {
            _sourceProviderRepositoryMock = new Mock<ISourceProviderRepository>();
            _appGuidFinderMock = new Mock<IApplicationGuidFinder>();
            _dataProviderFactoryFactoryMock = new Mock<IDataProviderFactoryFactory>();
            _loggerFake = new Mock<IAPILog>();
            _toggleProvider = new Mock<IToggleProvider>();

            _toggleProvider.Setup(x => x.IsEnabledByName(It.IsAny<string>())).Returns(false);

            _sut = new RipProviderInstaller(
                _loggerFake.Object,
                _sourceProviderRepositoryMock.Object,
                _appGuidFinderMock.Object,
                _dataProviderFactoryFactoryMock.Object,
                _toggleProvider.Object
            );

            SetupDataProviderFactoryFactory();
            SetupExistingSourceProviders(Enumerable.Empty<Guid>());

            _existingProviderApplicationGuid = Guid.NewGuid();
            _sourceProviderToCreate = new SourceProvider
            {
                Name = "TestProvider",
                ApplicationGUID = Guid.NewGuid(),
                ApplicationID = 431,
                GUID = _existingProviderApplicationGuid,
                Url = "http://rip.test.com/",
                ViewDataUrl = "http://rip.test.com/view",
                Configuration = new SourceProviderConfiguration
                {
                    AvailableImportSettings = new ImportSettingVisibility(),
                    CompatibleRdoTypes = new List<Guid> { Guid.NewGuid() }
                }
            };
            _sourceProvidersToCreate = new[] { _sourceProviderToCreate };
        }

        [Test]
        public async Task ShouldAddNewProvider()
        {
            // act
            Either<string, Unit> result = await _sut.InstallProvidersAsync(_sourceProvidersToCreate).ConfigureAwait(false);

            // assert
            result.Should().BeRight("because provider should be added");

            _sourceProviderRepositoryMock.Verify(z =>
                z.Create(
                    It.Is<Data.SourceProvider>(actualSourceProvider => AssertAreEqual(_sourceProviderToCreate, actualSourceProvider))
                )
            );
        }

        [Test]
        public async Task ShouldUpdateExistingProvider()
        {
            // arrange
            SetupExistingSourceProviders(new[] { _sourceProviderToCreate.GUID });

            // act
            Either<string, Unit> result = await _sut.InstallProvidersAsync(_sourceProvidersToCreate).ConfigureAwait(false);

            // assert
            result.Should().BeRight("because provider should be updated");

            SourceProvider expectedSourceProvider = _sourceProviderToCreate;
            expectedSourceProvider.ApplicationGUID = _existingProviderApplicationGuid;

            _sourceProviderRepositoryMock.Verify(z =>
                z.Update(
                    It.Is<Data.SourceProvider>(actualSourceProvider => AssertAreEqual(expectedSourceProvider, actualSourceProvider))
                )
            );
        }

        [Test]
        public async Task ShouldReturnErrorWhenCannotLoadProvider()
        {
            // arrange
            string exceptionMessage = "Provider not valid";
            var exceptionToThrow = new InvalidOperationException(exceptionMessage);

            SetupDataProviderFactoryFactory(exceptionToThrow);

            // act
            Either<string, Unit> result = await _sut.InstallProvidersAsync(_sourceProvidersToCreate).ConfigureAwait(false);

            // assert
            string expectedErrorMessage = $"Error while loading '{_sourceProviderToCreate.Name}' provider: {exceptionMessage}";
            result.Should().BeLeft(expectedErrorMessage, "because cannot load provider");
        }

        [Test]
        public async Task ShouldUpdateApplicationGuidWhenMissing()
        {
            // arrange
            _sourceProviderToCreate.ApplicationGUID = Guid.Empty;
            Guid expectedApplicationGuid = Guid.NewGuid();
            _appGuidFinderMock.Setup(x => x.GetApplicationGuid(It.IsAny<int>()))
                .Returns(expectedApplicationGuid);

            // act
            Either<string, Unit> result = await _sut.InstallProvidersAsync(_sourceProvidersToCreate).ConfigureAwait(false);

            // assert
            result.Should().BeRight("because application guid should be updated for provider");

            _sourceProviderToCreate.ApplicationGUID.Should().Be(expectedApplicationGuid, "because application guid should be updated");

            _sourceProviderRepositoryMock.Verify(z =>
                z.Create(
                    It.Is<Data.SourceProvider>(actualSourceProvider => AssertAreEqual(_sourceProviderToCreate, actualSourceProvider))
                )
            );
        }

        [Test]
        public async Task ShouldReturnErrorWhenCannotFindApplicationGuid()
        {
            // arrange
            var exceptionToThrow = new InvalidOperationException("Cannot find guid");

            _sourceProviderToCreate.ApplicationGUID = Guid.Empty;
            _appGuidFinderMock.Setup(x => x.GetApplicationGuid(It.IsAny<int>()))
                .Throws(exceptionToThrow);

            // act
            Either<string, Unit> result = await _sut.InstallProvidersAsync(_sourceProvidersToCreate).ConfigureAwait(false);

            // assert
            string expectedErrorMessage = $"Unhandled error occured while installing providers. Exception: {exceptionToThrow}";
            result.Should().BeLeft(expectedErrorMessage, "because cannot find application guid");
        }

        [Test]
        public async Task ShouldReturnErrorWhenCannotGetInstalledProviders()
        {
            // arrange
            var exceptionToThrow = new InvalidOperationException("Cannot get installed providers");
            _sourceProviderRepositoryMock
                .Setup(x => x.GetSourceProviderRdoByApplicationIdentifierAsync(It.IsAny<Guid>()))
                .Throws(exceptionToThrow);

            // act
            Either<string, Unit> result = await _sut.InstallProvidersAsync(_sourceProvidersToCreate).ConfigureAwait(false);

            // assert
            string expectedErrorMessage = $"Unhandled error occured while installing providers. Exception: {exceptionToThrow}";
            result.Should().BeLeft(expectedErrorMessage, "because cannot retrieve installed providers");
        }

        [Test]
        public async Task ShouldReturnErrorWhenSourceProvidersAreNull()
        {
            // act
            Either<string, Unit> result = await _sut.InstallProvidersAsync(null).ConfigureAwait(false);

            // assert
            result.Should().BeLeft("Argument 'providersToInstall' cannot be null", "because providers were null");
        }

        [Test]
        public void ShouldNotFailWhenThereIsDuplicatedSourceProvider()
        {
            // arrange
            const int numberOfDuplicates = 2;
            _sourceProviderRepositoryMock.Setup(x => x.GetSourceProviderRdoByApplicationIdentifierAsync(It.IsAny<Guid>()))
                .ReturnsAsync(
                    Enumerable
                        .Repeat(_existingProviderApplicationGuid, numberOfDuplicates)
                        .Select(x => new Data.SourceProvider()
                        {
                            Identifier = x.ToString()
                        })
                        .ToList()
                );

            RipProviderInstaller sut = new RipProviderInstaller(
                _loggerFake.Object,
                _sourceProviderRepositoryMock.Object,
                _appGuidFinderMock.Object,
                _dataProviderFactoryFactoryMock.Object,
                _toggleProvider.Object
            );

            // act
            Func<Task<Either<string, Unit>>> action = () => sut.InstallProvidersAsync(_sourceProvidersToCreate);

            // assert
            action.ShouldNotThrow();
            _loggerFake.Verify(x => x.LogWarning("There are duplicated entries in SourceProvider database table."));
        }

        private void SetupDataProviderFactoryFactory(Exception exceptionToThrow = null)
        {
            var providerFactoryVendorMock = new Mock<ProviderFactoryVendor>();

            _dataProviderFactoryFactoryMock
                .Setup(x => x.CreateProviderFactoryVendor())
                .Returns(Right<string, ProviderFactoryVendor>(providerFactoryVendorMock.Object));

            var dataProviderFactoryMock = new Mock<IDataProviderFactory>();

            _dataProviderFactoryFactoryMock
                .Setup(x => x.CreateDataProviderFactory(It.IsAny<ProviderFactoryVendor>()))
                .Returns(dataProviderFactoryMock.Object);

            ISetup<IDataProviderFactory, IDataSourceProvider> getDataProviderSetup =
                dataProviderFactoryMock.Setup(x => x.GetDataProvider(It.IsAny<Guid>(), It.IsAny<Guid>()));
            if (exceptionToThrow == null)
            {
                getDataProviderSetup.Returns((IDataSourceProvider)null);
            }
            else
            {
                getDataProviderSetup.Throws(exceptionToThrow);
            }
        }

        private void SetupExistingSourceProviders(IEnumerable<Guid> sourceProvidersGuids)
        {
            List<Data.SourceProvider> sourceProviders = sourceProvidersGuids
                .Select(guid => guid.ToString())
                .Select(CreateDataSourceProviderMock)
                .ToList();

            _sourceProviderRepositoryMock
                .Setup(x => x.GetSourceProviderRdoByApplicationIdentifierAsync(It.IsAny<Guid>()))
                .ReturnsAsync(sourceProviders);
        }

        private Data.SourceProvider CreateDataSourceProviderMock(string guidString)
        {
            // we have to initialize all properties, because otherwise exception will be thrown while accessing it
            return new Data.SourceProvider
            {
                Identifier = guidString,
                ApplicationIdentifier = _existingProviderApplicationGuid.ToString(),
                Name = string.Empty,
                Config = new SourceProviderConfiguration(),
                ArtifactId = 0,
                SourceConfigurationUrl = string.Empty,
                ViewConfigurationUrl = string.Empty
            };
        }

        private bool AssertAreEqual(SourceProvider expected, Data.SourceProvider actual)
        {
            expected.Name.Should().Be(actual.Name);
            expected.ApplicationGUID.ToString().Should().Be(actual.ApplicationIdentifier);
            expected.GUID.ToString().Should().Be(actual.Identifier);
            expected.Url.Should().Be(actual.SourceConfigurationUrl);
            expected.ViewDataUrl.Should().Be(actual.ViewConfigurationUrl);

            AssertAreEqual(expected.Configuration, actual.Config);

            return true;
        }

        private void AssertAreEqual(SourceProviderConfiguration expected, SourceProviderConfiguration actual)
        {
            expected.AlwaysImportNativeFileNames.Should().Be(actual.AlwaysImportNativeFileNames);
            expected.AlwaysImportNativeFiles.Should().Be(actual.AlwaysImportNativeFiles);
            expected.OnlyMapIdentifierToIdentifier.Should().Be(actual.OnlyMapIdentifierToIdentifier);
            expected.CompatibleRdoTypes.Should().BeEquivalentTo(actual.CompatibleRdoTypes);

            AssertAreEqual(expected.AvailableImportSettings, actual.AvailableImportSettings);
        }

        private void AssertAreEqual(ImportSettingVisibility expected, ImportSettingVisibility actual)
        {
            expected.AllowUserToMapNativeFileField.Should().Be(actual.AllowUserToMapNativeFileField);
        }
    }
}
