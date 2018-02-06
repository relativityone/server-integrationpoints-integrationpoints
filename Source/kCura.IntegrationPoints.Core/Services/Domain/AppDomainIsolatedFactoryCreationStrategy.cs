using System;
using System.Collections.Generic;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Domain;

namespace kCura.IntegrationPoints.Core.Services.Domain
{
	public class AppDomainIsolatedFactoryLifecycleStrategy : IProviderFactoryLifecycleStrategy
	{
		private readonly Dictionary<Guid, AppDomain> _appDomains;
		protected readonly IDomainHelper DomainHelper;

		public AppDomainIsolatedFactoryLifecycleStrategy(IDomainHelper domainHelper)
		{
			DomainHelper = domainHelper;

			_appDomains = new Dictionary<Guid, AppDomain>();
		}

		public virtual IProviderFactory CreateProviderFactory(Guid applicationId)
		{
			AppDomain newDomain = CreateNewDomain(applicationId);
			IDomainManager domainManager = DomainHelper.SetupDomainAndCreateManager(newDomain, applicationId);
			return domainManager.CreateProviderFactory();
		}

		private AppDomain CreateNewDomain(Guid applicationId)
		{
			AppDomain newDomain = DomainHelper.CreateNewDomain();
			_appDomains[applicationId] = newDomain;
			return newDomain;
		}

		public virtual void OnReleaseProviderFactory(Guid applicationId)
		{
			AppDomain domainToRelease;
			if (_appDomains.TryGetValue(applicationId, out domainToRelease))
			{
				DomainHelper.ReleaseDomain(domainToRelease);
				_appDomains.Remove(applicationId);
			}
		}
	}
}