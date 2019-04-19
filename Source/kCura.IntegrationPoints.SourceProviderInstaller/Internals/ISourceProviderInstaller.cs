using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Contracts;

namespace kCura.IntegrationPoints.SourceProviderInstaller.Internals
{
    internal interface ISourceProviderInstaller
    {
        Task InstallSourceProvidersAsync(int workspaceID, IEnumerable<SourceProvider> sourceProviders);
    }
}
