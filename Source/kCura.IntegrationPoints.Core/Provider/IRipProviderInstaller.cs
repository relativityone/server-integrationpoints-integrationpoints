using System.Collections.Generic;
using System.Threading.Tasks;
using LanguageExt;
using Relativity.IntegrationPoints.Contracts;

namespace kCura.IntegrationPoints.Core.Provider
{
    public interface IRipProviderInstaller
    {
        Task<Either<string, Unit>> InstallProvidersAsync(IEnumerable<SourceProvider> providersToInstall);
    }
}
