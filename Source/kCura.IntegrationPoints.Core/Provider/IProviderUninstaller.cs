using LanguageExt;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.Core.Provider
{
    public interface IProviderUninstaller
    {
        Task<Either<string, Unit>> UninstallProvidersAsync(int applicationID);
    }
}
