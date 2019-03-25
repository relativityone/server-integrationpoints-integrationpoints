﻿using System;
using Castle.Windsor;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Core.Services.Domain;
using kCura.IntegrationPoints.Domain;
using NSubstitute;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Tests.Services.Domain
{
	[TestFixture]
	public class ProviderFactoryCreationStrategyTests : TestBase
	{
		private IHelper _helper;
		private AppDomain _appDomainMock;
		private ProviderFactoryLifecycleStrategy _creationStrategy;
		private IWindsorContainerSetup _windsorContainerSetup;
		private IWindsorContainer _windsorContainer;
		private IAppDomainHelper _domainHelperMock;
		private IAppDomainManager _domainManagerMock;
		private IProviderFactory _providerFactoryMock;
		private readonly Guid _thirdPartyApplicationGuid = Guid.NewGuid();
		private readonly Guid _internalApplicationGuid = Guid.Parse(Constants.IntegrationPoints.APPLICATION_GUID_STRING);

		public override void SetUp()
		{
			_helper = Substitute.For<IHelper>();
			_domainHelperMock = Substitute.For<IAppDomainHelper>();
			_providerFactoryMock = Substitute.For<IProviderFactory>();
			_domainManagerMock = Substitute.For<IAppDomainManager>();
			_domainManagerMock.CreateProviderFactory().Returns(_providerFactoryMock);
			_appDomainMock = AppDomain.CreateDomain("AppDomainIsolatedFactoryCreationStrategyTestsAppDomain");
			_domainHelperMock.CreateNewDomain().Returns(_appDomainMock);
			_domainHelperMock.SetupDomainAndCreateManager(_appDomainMock, _thirdPartyApplicationGuid).Returns(_domainManagerMock);
			_windsorContainer = Substitute.For<IWindsorContainer>();
			_windsorContainerSetup = Substitute.For<IWindsorContainerSetup>();
			_windsorContainerSetup.SetUpCastleWindsor(_helper).Returns(_windsorContainer);
			_creationStrategy = new ProviderFactoryLifecycleStrategy(_helper, _domainHelperMock, _windsorContainerSetup);
		}



		[Test]
		public void CreateProviderFactory_InternalAppGuid_ReturnsInternalProviderFactory()
		{
			IProviderFactory factory = _creationStrategy.CreateProviderFactory(_internalApplicationGuid);

			Assert.IsTrue(factory.GetType() == typeof(InternalProviderFactory));
		}

		[Test]
		public void OnReleaseProviderFactory_InternalAppGuid_DisposesContainer()
		{
			_creationStrategy.CreateProviderFactory(_internalApplicationGuid);

			_creationStrategy.OnReleaseProviderFactory(_internalApplicationGuid);

			_windsorContainer.Received().Dispose();
		}

		[Test]
		public void CreateProviderFactory_CallsSetupDomainAndCreateManager()
		{
			_creationStrategy.CreateProviderFactory(_thirdPartyApplicationGuid);

			_domainHelperMock.Received().SetupDomainAndCreateManager(_appDomainMock, _thirdPartyApplicationGuid);
		}

		[Test]
		public void OnReleaseProviderFactory_GetsExistingAppGuidGuid_CallsReleaseDomain()
		{
			_creationStrategy.CreateProviderFactory(_thirdPartyApplicationGuid);

			_creationStrategy.OnReleaseProviderFactory(_thirdPartyApplicationGuid);

			_domainHelperMock.Received().ReleaseDomain(_appDomainMock);
		}
	}
}
