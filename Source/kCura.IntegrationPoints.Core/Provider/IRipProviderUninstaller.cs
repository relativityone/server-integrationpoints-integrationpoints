using LanguageExt;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Provider
{
    public interface IRipProviderUninstaller
    {
        Task<Either<string, Unit>> UninstallProvidersAsync(int applicationID);
    }
}
