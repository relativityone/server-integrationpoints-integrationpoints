using System;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Domain;
using kCura.IntegrationPoints.Domain.Logging;

namespace kCura.IntegrationPoints.Core.Services.Domain
{
	public class AppDomainFactory : IDataProviderFactory, IDisposable
	{
		private AppDomain _newDomain;
		private readonly IDomainHelper _domainHelper;

		public AppDomainFactory(IDomainHelper domainHelper)
		{
			_domainHelper = domainHelper;
		}

		public IDataSourceProvider GetDataProvider(Guid applicationGuid, Guid providerGuid)
		{
			using (new SerilogContextRestorer())
			{
				_newDomain = _domainHelper.CreateNewDomain();
				IDomainManager domainManager = _domainHelper.SetupDomainAndCreateManager(_newDomain, applicationGuid);

				IProviderFactory providerFactory = domainManager.CreateProviderFactory();
				
				return new ProviderWithLogContextDecorator(providerFactory.CreateProvider(providerGuid));
			}
		}

		public void Dispose()
		{
			_domainHelper.ReleaseDomain(_newDomain);
		}
	}
}