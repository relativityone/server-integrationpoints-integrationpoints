using System;
using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Core.Services.Provider
{
	public class MockProviderFactory : IDataProviderFactory
	{
		public IDataSourceProvider GetDataProvider()
		{
			//return new kCura.IntegrationPoints.Core.Services.Provider.
			throw new NotImplementedException();
		}
	}
}
