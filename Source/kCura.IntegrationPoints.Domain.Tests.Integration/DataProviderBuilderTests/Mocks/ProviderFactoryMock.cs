using System;
using Relativity.IntegrationPoints.Contracts.Provider;
using Relativity.IntegrationPoints.SourceProviderInstaller;

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
