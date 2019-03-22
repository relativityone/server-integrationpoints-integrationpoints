﻿using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Core.Services.Domain;
using kCura.IntegrationPoints.Domain;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Services.Domain
{
	[TestFixture]
	public class AppDomainIsolatedFactoryCreationStrategyTests : TestBase
	{
		private AppDomain _appDomainMock;
		private AppDomainIsolatedFactoryLifecycleStrategy _creationStrategy;
		private IAppDomainHelper _domainHelperMock;
		private IAppDomainManager _domainManagerMock;
		private IProviderFactory _providerFactoryMock;
		private readonly Guid _applicationGuid = Guid.NewGuid();

		public override void SetUp()
		{
			_domainHelperMock = Substitute.For<IAppDomainHelper>();
			_providerFactoryMock = Substitute.For<IProviderFactory>();
			_domainManagerMock = Substitute.For<IAppDomainManager>();
			_domainManagerMock.CreateProviderFactory().Returns(_providerFactoryMock);
			_appDomainMock = AppDomain.CreateDomain("AppDomainIsolatedFactoryCreationStrategyTestsAppDomain");
			_domainHelperMock.CreateNewDomain().Returns(_appDomainMock);
			_domainHelperMock.SetupDomainAndCreateManager(_appDomainMock, _applicationGuid).Returns(_domainManagerMock);
			_creationStrategy = new AppDomainIsolatedFactoryLifecycleStrategy(_domainHelperMock);
		}


		[Test]
		public void CreateProviderFactory_CallsSetupDomainAndCreateManager()
		{
			_creationStrategy.CreateProviderFactory(_applicationGuid);

			_domainHelperMock.Received().SetupDomainAndCreateManager(_appDomainMock, _applicationGuid);
		}

		[Test]
		public void OnReleaseProviderFactory_GetsExistingAppGuidGuid_CallsReleaseDomain()
		{
			_creationStrategy.CreateProviderFactory(_applicationGuid);

			_creationStrategy.OnReleaseProviderFactory(_applicationGuid);

			_domainHelperMock.Received().ReleaseDomain(_appDomainMock);
		}

		[Test]
		public void OnReleaseProviderFactory_GetsNonExistingAppGuidGuid_DoNotCallReleaseDomain()
		{
			Guid nonExistingApplicationGuid = Guid.NewGuid();
			_creationStrategy.CreateProviderFactory(_applicationGuid);

			_creationStrategy.OnReleaseProviderFactory(nonExistingApplicationGuid);

			_domainHelperMock.DidNotReceive().ReleaseDomain(_appDomainMock);
		}

		[Test]
		public void OnReleaseProviderFactory_ManyAppGuidsInCache_DoNotCallReleaseDomainForSpecificGuid()
		{
			Guid otherApplicationGuid = Guid.NewGuid();
			IAppDomainManager otherDomainManagerMock = Substitute.For<IAppDomainManager>();
			otherDomainManagerMock.CreateProviderFactory().Returns(_providerFactoryMock);
			AppDomain otherAppDomainMock = AppDomain.CreateDomain("AppDomainIsolatedFactoryCreationStrategyTestsAppDomain");

			_creationStrategy.CreateProviderFactory(_applicationGuid);
			_domainHelperMock.CreateNewDomain().Returns(otherAppDomainMock);
			_domainHelperMock.SetupDomainAndCreateManager(otherAppDomainMock, otherApplicationGuid).Returns(otherDomainManagerMock);
			_creationStrategy.CreateProviderFactory(otherApplicationGuid);

			_creationStrategy.OnReleaseProviderFactory(otherApplicationGuid);

			_domainHelperMock.DidNotReceive().ReleaseDomain(_appDomainMock);
		}
	}
}
