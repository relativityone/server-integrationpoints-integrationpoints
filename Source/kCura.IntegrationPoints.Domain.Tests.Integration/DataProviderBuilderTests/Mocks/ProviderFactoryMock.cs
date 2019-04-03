using System;
using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Domain.Tests.Integration.DataProviderBuilderTests.Mocks
{
	internal class ProviderFactoryMock : ProviderFactoryBase
	{
		public override IDataSourceProvider CreateInstance(Type providerType)
		{
			return new DataSourceProviderMock();
		}
	}
}
