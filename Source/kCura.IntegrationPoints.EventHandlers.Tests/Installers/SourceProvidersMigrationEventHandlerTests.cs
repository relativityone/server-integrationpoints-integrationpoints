using FluentAssertions;
using kCura.EventHandler;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Provider;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints;
using kCura.IntegrationPoints.Services;
using LanguageExt;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static LanguageExt.Prelude;
using SourceProvider = kCura.IntegrationPoints.Contracts.SourceProvider;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Installers
{
    /// <summary>
    /// Objective : this test suite is to test the simulation of source provider install eventhandler during migration.
    /// These set of tests verify that all source providers are passed in to <see cref="IProviderManager"/> properly.
    /// </summary>
    [TestFixture]
    internal class SourceProvidersMigrationEventHandlerTests : SourceProvidersMigrationEventHandler
    {
        private List<Data.SourceProvider> _providersStub;
        private Mock<IProviderInstaller> _providerInstallerMock;
        private static readonly Mock<IErrorService> _errorServiceMock = new Mock<IErrorService>();

        public SourceProvidersMigrationEventHandlerTests() : base(_errorServiceMock.Object)
        { }

        [OneTimeSetUp]
        public void Setup()
        {
            _providerInstallerMock = new Mock<IProviderInstaller>();
            _providerInstallerMock.Setup(x => x.InstallProvidersAsync(It.IsAny<IEnumerable<SourceProvider>>()))
                .Returns(Task.FromResult(Right<string, Unit>(Unit.Default)));

            var helper = new Mock<IEHHelper>
            {
                DefaultValue = DefaultValue.Mock
            };

            Helper = helper.Object;
            ProviderInstallerForTests = _providerInstallerMock.Object;
        }

        protected override List<Data.SourceProvider> GetSourceProvidersFromPreviousWorkspace()
        {
            return _providersStub;
        }

        [Test]
        public void NoProviderInPreviousWorkspace()
        {
            // arrange
            _providersStub = new List<Data.SourceProvider>();

            // act
            Response result = Execute();

            // assert
            result.Success.Should().BeFalse("because provider was not present in previous workspace");
            _errorServiceMock
                .Verify(x =>
                    x.Log(It.Is<ErrorModel>(error => error.Message == "Failed to migrate Source Provider.")),
                    "because no providers were installed in a previous workspace"
                );
        }

        [Test]
        [SmokeTest]
        public void OneProviderInPreviousWorkspace()
        {
            Guid identifier = new Guid("e01ff2d2-2ac7-4390-bbc3-64c6c17758bc");
            Guid appIdentifier = new Guid("7cd4c64f-747b-4962-9647-671ee65b6ea4");
            const int artifactId = 798;
            const string name = "Test";
            const string url = "fake url";
            const string dataUrl = "config url";

            // arrange
            var providerToInstalled = new Data.SourceProvider
            {
                ApplicationIdentifier = appIdentifier.ToString(),
                Identifier = identifier.ToString(),
                ArtifactId = artifactId,
                Name = name,
                SourceConfigurationUrl = url,
                ViewConfigurationUrl = dataUrl,
                Config = new SourceProviderConfiguration()
            };

            _providersStub = new List<Data.SourceProvider>
            {
                providerToInstalled
            };

            // act
            Response result = Execute();

            // assert
            result.Success.Should().BeTrue();

            VerifyCorrectNumberOfProviderWasInstalledUsingProductionManager(_providersStub.Count);
            VerifyProviderWasInstalledUsingProductionManager(providerToInstalled.Name);
        }

        [Test]
        public void MultipleProvidersInPreviousWorkspace()
        {
            // arrange
            var providerToInstalled = new Data.SourceProvider
            {
                ApplicationIdentifier = "72194851-ad15-4769-bec5-04011498a1b4",
                Identifier = "e01ff2d2-2ac7-4390-bbc3-64c6c17758bc",
                ArtifactId = 789,
                Name = "test",
                SourceConfigurationUrl = "fake url",
                ViewConfigurationUrl = "config url",
                Config = new SourceProviderConfiguration()
            };

            var provider2ToInstalled = new Data.SourceProvider
            {
                ApplicationIdentifier = "cf3ab0f2-d26f-49fb-bd11-547423a692c1",
                Identifier = "e01ff2d2-2ac7-4390-bbc3-64c6c17758bd",
                ArtifactId = 777,
                Name = "test2",
                SourceConfigurationUrl = "fake url2",
                ViewConfigurationUrl = "config url2",
                Config = new SourceProviderConfiguration()
            };

            _providersStub = new List<Data.SourceProvider>
            {
                providerToInstalled,
                provider2ToInstalled
            };

            // act
            Response result = Execute();

            //assert
            result.Success.Should().BeTrue();


            VerifyCorrectNumberOfProviderWasInstalledUsingProductionManager(_providersStub.Count);
            VerifyProviderWasInstalledUsingProductionManager(providerToInstalled.Name);
            VerifyProviderWasInstalledUsingProductionManager(provider2ToInstalled.Name);
        }

        private void VerifyCorrectNumberOfProviderWasInstalledUsingProductionManager(int expectedNumberOfProviders)
        {
            string failureMessage = $"because {expectedNumberOfProviders} provider was installed in previous workspace";
            bool Predicate(IEnumerable<SourceProvider> sourceProviders) => sourceProviders.Count() == expectedNumberOfProviders;

            VerifyInstallationUsingProviderInstaller(failureMessage, Predicate);
        }

        private void VerifyProviderWasInstalledUsingProductionManager(string providerName)
        {
            string failureMessage = $"because '{providerName}' provider was installed in previous workspace";

            bool Predicate(IEnumerable<SourceProvider> sourceProviders) =>
                sourceProviders.SingleOrDefault(p => p.Name == providerName) != null;

            VerifyInstallationUsingProviderInstaller(failureMessage, Predicate);
        }

        private void VerifyInstallationUsingProviderInstaller(
            string failureMessage,
            Func<IEnumerable<SourceProvider>, bool> predicate)
        {
            _providerInstallerMock
                .Verify(x =>
                        x.InstallProvidersAsync(It.Is<IEnumerable<SourceProvider>>(request => predicate(request))),
                    failureMessage
                );
        }
    }
}