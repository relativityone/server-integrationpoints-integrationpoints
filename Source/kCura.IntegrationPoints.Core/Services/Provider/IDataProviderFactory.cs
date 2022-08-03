using System;
using Relativity.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Core.Services.Provider
{
    public interface IDataProviderFactory
    {
        IDataSourceProvider GetDataProvider(Guid applicationGuid, Guid providerGuid);
    }
}
