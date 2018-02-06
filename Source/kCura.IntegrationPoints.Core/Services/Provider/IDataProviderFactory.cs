using System;
using kCura.IntegrationPoints.Contracts.Provider;
using Relativity.API;

namespace kCura.IntegrationPoints.Core.Services.Provider
{
	public interface IDataProviderFactory
	{
		IDataSourceProvider GetDataProvider(Guid applicationGuid, Guid providerGuid);
	}
}
