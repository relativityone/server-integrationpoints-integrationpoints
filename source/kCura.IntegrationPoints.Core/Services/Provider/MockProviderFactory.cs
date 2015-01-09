using System;
using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Core.Services.Provider
{
	public class MockProviderFactory : IDataProviderFactory
	{
		public IDataSourceProvider GetDataProvider(Guid selector)
		{
			//in the future this will look against the app domain and load an interface that will proxy data but for now
			//it just returns the provider
			return new kCura.IntegrationPoints.LDAPProvider.LDAPProvider();
		}
	}
}
