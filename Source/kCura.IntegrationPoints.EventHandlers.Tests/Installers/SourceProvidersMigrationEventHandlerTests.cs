using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.EventHandler;
using kCura.IntegrationPoint.Tests.Core.TestCategories.Attributes;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Provider;
using kCura.IntegrationPoints.Core.Services;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints;
using LanguageExt;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Services;
using static LanguageExt.Prelude;

namespace kCura.IntegrationPoints.EventHandlers.Tests.Installers
{
    /// <summary>
    /// Objective : this test suite is to test the simulation of source provider install eventhandler during migration.
    /// These set of tests verify that all source providers are passed in to <see cref="IProviderManager"/> properly.
    /// </summary>
    [TestFixture, Category("Unit")]
    public class SourceProvidersMigrationEventHandlerTests
    {
        private SubjectUnderTests _sut;

        private Mock<IRipProviderInstaller> _providerInstallerMock;
        private Mock<IErrorService> _errorServiceMock;
        private Mock<IAPILog> _loggerMock;
        private Mock<IKubernetesMode> _kubernetesModeMock;

        [OneTimeSetUp]
        public void Setup()
        {
	        _loggerMock = new Mock<IAPILog>();
            _errorServiceMock = new Mock<IErrorService>();
            _providerInstallerMock = new Mock<IRipProviderInstaller>();
            _kubernetesModeMock = new Mock<IKubernetesMode>();
            _providerInstallerMock.Setup(x => x.InstallProvidersAsync(It.IsAny<IEnumerable<SourceProvider>>()))
                .Returns(Task.FromResult(Right<string, Unit>(Unit.Default)));

            var helper = new Mock<IEHHelper>
            {
                DefaultValue = DefaultValue.Mock
            };
            helper.Setup(x => x.GetLoggerFactory().GetLogger().ForContext<DataTransferLocationMigrationEventHandler>())
	            .Returns(_loggerMock.Object);

            _sut = new SubjectUnderTests(
                helper.Object,
                _errorServiceMock.Object,
                _providerInstallerMock.Object,
                _kubernetesModeMock.Object
            );
        }

        [Test]
        public void NoProviderInPreviousWorkspace()
        {
            // arrange
            _sut.SourceProvidersToReturn = new List<Data.SourceProvider>();

            // act
            Response result = _sut.Execute();

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
            var providerToInstall = new Data.SourceProvider
            {
                ApplicationIdentifier = appIdentifier.ToString(),
                Identifier = identifier.ToString(),
                ArtifactId = artifactId,
                Name = name,
                SourceConfigurationUrl = url,
                ViewConfigurationUrl = dataUrl,
                Config = new SourceProviderConfiguration()
            };

            var providersToInstall = new List<Data.SourceProvider>
            {
                providerToInstall
            };

            _sut.SourceProvidersToReturn = providersToInstall;

            // act
            Response result = _sut.Execute();

            // assert
            result.Success.Should().BeTrue();

            VerifyCorrectNumberOfProviderWasInstalledUsingProductionManager(providersToInstall.Count);
            VerifyProviderWasInstalledUsingProductionManager(providerToInstall.Name);
        }

        [Test]
        public void MultipleProvidersInPreviousWorkspace()
        {
            // arrange
            var providerToInstall = new Data.SourceProvider
            {
                ApplicationIdentifier = "72194851-ad15-4769-bec5-04011498a1b4",
                Identifier = "e01ff2d2-2ac7-4390-bbc3-64c6c17758bc",
                ArtifactId = 789,
                Name = "test",
                SourceConfigurationUrl = "fake url",
                ViewConfigurationUrl = "config url",
                Config = new SourceProviderConfiguration()
            };

            var provider2ToInstall = new Data.SourceProvider
            {
                ApplicationIdentifier = "cf3ab0f2-d26f-49fb-bd11-547423a692c1",
                Identifier = "e01ff2d2-2ac7-4390-bbc3-64c6c17758bd",
                ArtifactId = 777,
                Name = "test2",
                SourceConfigurationUrl = "fake url2",
                ViewConfigurationUrl = "config url2",
                Config = new SourceProviderConfiguration()
            };

            var providersToInstall = new List<Data.SourceProvider>
            {
                providerToInstall,
                provider2ToInstall
            };
            _sut.SourceProvidersToReturn = providersToInstall;

            // act
            Response result = _sut.Execute();

            //assert
            result.Success.Should().BeTrue();


            VerifyCorrectNumberOfProviderWasInstalledUsingProductionManager(providersToInstall.Count);
            VerifyProviderWasInstalledUsingProductionManager(providerToInstall.Name);
            VerifyProviderWasInstalledUsingProductionManager(provider2ToInstall.Name);
        }

        [Test]
        public void ShouldNotFailWhenDuplicatedSourceProvidersExistsInTemplateWorkspace()
        {
            // arrange
            var providerToInstall = new Data.SourceProvider
            {
	            ApplicationIdentifier = "72194851-ad15-4769-bec5-04011498a1b4",
	            Identifier = "e01ff2d2-2ac7-4390-bbc3-64c6c17758bc",
	            ArtifactId = 789,
	            Name = "test",
	            SourceConfigurationUrl = "fake url",
	            ViewConfigurationUrl = "config url",
	            Config = new SourceProviderConfiguration()
            };
            var providersToInstall = new List<Data.SourceProvider>
            {
	            providerToInstall,
	            providerToInstall
            };
            _sut.SourceProvidersToReturn = providersToInstall;

            // act
            Response result = _sut.Execute();

            //assert
            result.Success.Should().BeTrue();
            VerifyCorrectNumberOfProviderWasInstalledUsingProductionManager(1);
            VerifyProviderWasInstalledUsingProductionManager(providerToInstall.Name);

            _loggerMock.Verify(x => x.LogWarning("There are duplicated entries in SourceProvider database table in Template Workspace Artifact ID: {templateWorkspaceArtifactId}", It.IsAny<int>()));
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

        private class SubjectUnderTests : SourceProvidersMigrationEventHandler
        {
            public List<Data.SourceProvider> SourceProvidersToReturn { get; set; }

            public SubjectUnderTests(
                IEHHelper helper,
                IErrorService errorService,
                IRipProviderInstaller ripProviderInstaller,
                IKubernetesMode kubernetesMode)
            : base(errorService, ripProviderInstaller, kubernetesMode)
            {
                Helper = helper;
            }

            protected override List<Data.SourceProvider> GetSourceProvidersFromPreviousWorkspace()
            {
                return SourceProvidersToReturn;
            }
        }
    }
}