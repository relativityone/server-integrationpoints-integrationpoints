using System.Threading.Tasks;

namespace Relativity.Sync.Transfer.FileMovementService
{
    public interface IFmsInstanceSettingsService
    {
        Task<string> GetKubernetesServicesUrl();

        Task<string> GetFileMovementServiceUrl();
    }
}
