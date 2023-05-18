using System.Threading.Tasks;
using Relativity.IntegrationPoints.Contracts.Provider;

namespace kCura.IntegrationPoints.Agent.CustomProvider.Services.SourceProvider
{
    public interface ISourceProviderService
    {
        Task<IDataSourceProvider> GetSourceProviderAsync(int workspaceId, int sourceProviderId);
    }
}