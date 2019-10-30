﻿using System;
using Castle.Windsor;
using kCura.IntegrationPoints.Domain;
using Relativity.API;
using Relativity.IntegrationPoints.Contracts;

namespace kCura.IntegrationPoints.Core.Services.Domain
{
	public class ProviderFactoryLifecycleStrategy : AppDomainIsolatedFactoryLifecycleStrategy
	{

		private IWindsorContainer _windsorContainer;
		private readonly Guid _applicationId;
		private readonly IHelper _helper;
		private readonly IWindsorContainerSetup _windsorContainerSetup;

		public ProviderFactoryLifecycleStrategy(IHelper helper, IAppDomainHelper domainHelper, IWindsorContainerSetup windsorContainerSetup) : base(domainHelper)
		{
			_helper = helper;
			_applicationId = Guid.Parse(Constants.IntegrationPoints.APPLICATION_GUID_STRING);
			_windsorContainerSetup = windsorContainerSetup;
		}

		public override IProviderFactory CreateProviderFactory(Guid applicationId)
		{
			if (applicationId == _applicationId)
			{
				DomainHelper.LoadClientLibraries(AppDomain.CurrentDomain, applicationId);
				return CreateInternalProviderFactory();
			}
			return base.CreateProviderFactory(applicationId);
		}

		public override void OnReleaseProviderFactory(Guid applicationId)
		{
			if (applicationId == _applicationId)
			{
				IWindsorContainer container = _windsorContainer;
				_windsorContainer = null;
				container.Dispose();
			}
			else
			{
				base.OnReleaseProviderFactory(applicationId);
			}
		}

		private IWindsorContainer SetupCastleWindsor()
		{
			return _windsorContainerSetup.SetUpCastleWindsor(_helper);
		}

		private IWindsorContainer GetWindsorContainer()
		{
			return _windsorContainer ?? (_windsorContainer = SetupCastleWindsor());
		}

		private IProviderFactory CreateInternalProviderFactory()
		{
			return new InternalProviderFactory(GetWindsorContainer());
		}
	}
}