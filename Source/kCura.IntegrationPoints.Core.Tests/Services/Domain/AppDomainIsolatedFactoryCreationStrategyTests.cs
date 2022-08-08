using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Services.Domain;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.EnvironmentalVariables;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts;

namespace kCura.IntegrationPoints.Core.Tests.Services.Domain
{
    [TestFixture, Category("Unit")]
    public class AppDomainIsolatedFactoryCreationStrategyTests : TestBase
    {
        private AppDomain _appDomainMock;
        private AppDomainIsolatedFactoryLifecycleStrategy _sut;
        private IAppDomainHelper _domainHelperMock;
        private IAppDomainManager _domainManagerMock;
        private IProviderFactory _providerFactoryMock;
        private readonly Guid _applicationGuid = Guid.NewGuid();
        private IKubernetesMode _kubernetesModeMock;
        private IHelper _helperMock;
        private IAPILog _loggerMock;


        public override void SetUp()
        {
            _domainHelperMock = Substitute.For<IAppDomainHelper>();
            _providerFactoryMock = Substitute.For<IProviderFactory>();
            _domainManagerMock = Substitute.For<IAppDomainManager>();
            _domainManagerMock.CreateProviderFactory().Returns(_providerFactoryMock);
            _appDomainMock = AppDomain.CreateDomain("AppDomainIsolatedFactoryCreationStrategyTestsAppDomain");
            _domainHelperMock.CreateNewDomain().Returns(_appDomainMock);
            _domainHelperMock.SetupDomainAndCreateManager(_appDomainMock, _applicationGuid).Returns(_domainManagerMock);

            _kubernetesModeMock = Substitute.For<IKubernetesMode>();
            _kubernetesModeMock.IsEnabled().Returns(false);

            _loggerMock = Substitute.For<IAPILog>();
            _helperMock = Substitute.For<IHelper>();
            _helperMock.GetLoggerFactory().GetLogger().ForContext<AppDomainIsolatedFactoryLifecycleStrategy>()
                .Returns(_loggerMock);

            _sut =
                new AppDomainIsolatedFactoryLifecycleStrategy(_domainHelperMock, _kubernetesModeMock, _helperMock);
        }


        [Test]
        public void CreateProviderFactory_CallsSetupDomainAndCreateManager()
        {
            _sut.CreateProviderFactory(_applicationGuid);

            _domainHelperMock.Received().SetupDomainAndCreateManager(_appDomainMock, _applicationGuid);
        }

        [Test]
        public void OnReleaseProviderFactory_GetsExistingAppGuidGuid_CallsReleaseDomain()
        {
            _sut.CreateProviderFactory(_applicationGuid);

            _sut.OnReleaseProviderFactory(_applicationGuid);

            _domainHelperMock.Received().ReleaseDomain(_appDomainMock);
        }

        [Test]
        public void OnReleaseProviderFactory_GetsNonExistingAppGuidGuid_DoNotCallReleaseDomain()
        {
            Guid nonExistingApplicationGuid = Guid.NewGuid();
            _sut.CreateProviderFactory(_applicationGuid);

            _sut.OnReleaseProviderFactory(nonExistingApplicationGuid);

            _domainHelperMock.DidNotReceive().ReleaseDomain(_appDomainMock);
        }

        [Test]
        public void OnReleaseProviderFactory_ManyAppGuidsInCache_DoNotCallReleaseDomainForSpecificGuid()
        {
            Guid otherApplicationGuid = Guid.NewGuid();
            IAppDomainManager otherDomainManagerMock = Substitute.For<IAppDomainManager>();
            otherDomainManagerMock.CreateProviderFactory().Returns(_providerFactoryMock);
            AppDomain otherAppDomainMock = AppDomain.CreateDomain("AppDomainIsolatedFactoryCreationStrategyTestsAppDomain");

            _sut.CreateProviderFactory(_applicationGuid);
            _domainHelperMock.CreateNewDomain().Returns(otherAppDomainMock);
            _domainHelperMock.SetupDomainAndCreateManager(otherAppDomainMock, otherApplicationGuid).Returns(otherDomainManagerMock);
            _sut.CreateProviderFactory(otherApplicationGuid);

            _sut.OnReleaseProviderFactory(otherApplicationGuid);

            _domainHelperMock.DidNotReceive().ReleaseDomain(_appDomainMock);
        }

        [Test]
        public void CreateProviderFactory_ShouldUseCurrentDomain_WhenIsKubernetesMode()
        {
            // Arrange
            _kubernetesModeMock.IsEnabled().Returns(true);
            
            // Act
            _sut.CreateProviderFactory(_applicationGuid);
            
            // Assert
            _domainHelperMock.Received(1).SetupDomainAndCreateManager(AppDomain.CurrentDomain, _applicationGuid);
        }
    }
}
