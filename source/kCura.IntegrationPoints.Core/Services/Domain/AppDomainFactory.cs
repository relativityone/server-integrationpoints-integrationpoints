﻿using System;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Domain;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Domain
{
	public class AppDomainFactory : IDataProviderFactory, IDisposable
	{
		private readonly DomainHelper _domainHelper;
		private readonly ISourcePluginProvider _provider;
		private AppDomain _newDomain;
		private RelativityFeaturePathService _relativityFeaturePathService;

		public AppDomainFactory(
			DomainHelper domainHelper, 
			ISourcePluginProvider provider, 
			RelativityFeaturePathService relativityFeaturePathService)
		{
			_domainHelper = domainHelper;
			_provider = provider;
			_relativityFeaturePathService = relativityFeaturePathService;
		}

		public IDataSourceProvider GetDataProvider(Guid applicationGuid, Guid providerGuid, IHelper helper)
		{
			_newDomain = _domainHelper.CreateNewDomain(_relativityFeaturePathService);
			DomainManager domainManager = _domainHelper.SetupDomainAndCreateManager(_newDomain, _provider, applicationGuid);

			return domainManager.GetProvider(providerGuid, helper);
		}

		public void Dispose()
		{
			_domainHelper.ReleaseDomain(_newDomain);
		}
	}
}
