using System;
using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Domain.Logging;

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
			using (new SerilogContextRestorer())
			{
				IProviderFactory providerFactory = _providerFactoryVendor.GetProviderFactory(applicationGuid);
				return new ProviderWithLogContextDecorator(providerFactory.CreateProvider(providerGuid));
			}
		}
	}
}