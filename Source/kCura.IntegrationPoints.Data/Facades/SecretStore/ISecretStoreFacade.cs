using System.Threading.Tasks;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Facades.SecretStore
{
    public interface ISecretStoreFacade
    {
        Task<Secret> GetAsync(string path);
        Task SetAsync(string path, Secret secret);
        Task DeleteAsync(string path);
    }
}
