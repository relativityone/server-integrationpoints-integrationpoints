using System;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Services.Provider;

namespace kCura.IntegrationPoints.Core.Domain
{
	public class AppDomainFactory : IDataProviderFactory, IDisposable
	{
		private readonly DomainHelper _helper;
		private readonly ISourcePluginProvider _provider;
		private AppDomain _newDomain;
		private RelativityFeaturePathService _relativityFeaturePathService;

		public AppDomainFactory(
			DomainHelper helper, 
			ISourcePluginProvider provider, 
			RelativityFeaturePathService relativityFeaturePathService)
		{
			_helper = helper;
			_provider = provider;
			_relativityFeaturePathService = relativityFeaturePathService;
		}

		public IDataSourceProvider GetDataProvider(Guid applicationGuid, Guid providerGuid)
		{
			_newDomain = _helper.CreateNewDomain(_relativityFeaturePathService);
			DomainManager domainManager = _helper.SetupDomainAndCreateManager(_newDomain, _provider, applicationGuid);

			return domainManager.GetProvider(providerGuid);
		}

		public void Dispose()
		{
			_helper.ReleaseDomain(_newDomain);
		}
	}
}
