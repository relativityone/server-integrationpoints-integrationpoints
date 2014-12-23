using kCura.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Core.Services.Provider
{
	public interface IDataProviderFactory
	{
		IDataSourceProvider GetDataProvider();
	}
}
