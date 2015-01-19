using System;
using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Core.Services.Provider
{
	public class AppDomainFactory : IDataProviderFactory, IDisposable
	{
		private readonly DomainHelper _helper;
		private readonly ISourcePluginProvider _provider;
		private AppDomain _newDomain;

		public AppDomainFactory(DomainHelper helper, ISourcePluginProvider provider)
		{
			_helper = helper;
			_provider = provider;
		}

		public IDataSourceProvider GetDataProvider(Guid applicationGuid, Guid providerGuid)
		{
			_newDomain = _helper.CreateNewDomain();
			var manager = _helper.SetupDomainAndCreateManager(_newDomain, _provider, applicationGuid);
			return manager.GetProvider(providerGuid);
		}

		public void Dispose()
		{
			_helper.ReleaseDomain(_newDomain);
		}
	}
}
