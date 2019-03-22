using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Services.Provider;
using kCura.IntegrationPoints.Domain.Wrappers;
using System;

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

			IDataSourceProvider provider = providerFactory.CreateProvider(providerGuid);
			return WrapDataProviderInSafeDisposeDecorator(provider);
		}

		private static IDataSourceProvider WrapDataProviderInSafeDisposeDecorator(IDataSourceProvider provider)
		{
			var providedAggregatedInterfaces = provider as IProviderAggregatedInterfaces;
			return providedAggregatedInterfaces != null
				? new SafeDisposingProviderWrapper(providedAggregatedInterfaces)
				: provider;
		}
	}
}