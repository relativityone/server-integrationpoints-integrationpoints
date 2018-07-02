using System;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Services.Provider;

namespace kCura.IntegrationPoints.Core.Services.Domain
{
	internal class DataProviderBuilder : IDataProviderFactory
	{
		private readonly ProviderFactoryVendor _providerFactoryVendor;

		public DataProviderBuilder(ProviderFactoryVendor providerFactoryVendor)
		{
			_providerFactoryVendor = providerFactoryVendor;
		}

		public IDataSourceProvider GetDataProvider(Guid applicationGuid, Guid providerGuid)
		{
			IProviderFactory providerFactory = _providerFactoryVendor.GetProviderFactory(applicationGuid);
			return providerFactory.CreateProvider(providerGuid);
		}
	}
}