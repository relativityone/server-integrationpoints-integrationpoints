using System.Threading.Tasks;

namespace Relativity.Sync.Transfer.FileMovementService
{
    internal interface IFmsInstanceSettingsService
    {
        Task<string> GetKubernetesServicesUrl();

        Task<string> GetFileMovementServiceUrl();
    }
}
