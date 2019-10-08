using kCura.IntegrationPoints.Contracts;
using kCura.IntegrationPoints.Contracts.Provider;
using kCura.IntegrationPoints.Core.Services.Provider;
using System;
using kCura.IntegrationPoints.Contracts.Internals;
using kCura.IntegrationPoints.Contracts.Internals.Exceptions;
using kCura.IntegrationPoints.Domain.Exceptions;
using kCura.IntegrationPoints.SourceProviderInstaller.Internals.Wrappers;

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
			IDataSourceProvider provider = CreateProvider(applicationGuid, providerGuid);
			return WrapDataProviderInSafeDisposeDecorator(provider);
		}

		private IDataSourceProvider CreateProvider(Guid applicationGuid, Guid providerGuid)
		{
			IProviderFactory providerFactory = _providerFactoryVendor.GetProviderFactory(applicationGuid);
			IDataSourceProvider provider;
			try
			{
				provider = providerFactory.CreateProvider(providerGuid);
			}
			catch (TooManyProvidersFoundException e)
			{
				throw new IntegrationPointsException(string.Format(
					IntegrationPoints.Contracts.Properties.Resources.MoreThanOneProviderFound, e.ProviderCount, e.Identifier))
				{
					ExceptionSource = IntegrationPointsExceptionSource.EVENT_HANDLER
				};
			}
			catch (NoProvidersFoundException e)
			{
				throw new IntegrationPointsException(
					string.Format(IntegrationPoints.Contracts.Properties.Resources.NoProvidersFound, e.Identifier))
				{
					ExceptionSource = IntegrationPointsExceptionSource.EVENT_HANDLER
				};
			}

			return provider;
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