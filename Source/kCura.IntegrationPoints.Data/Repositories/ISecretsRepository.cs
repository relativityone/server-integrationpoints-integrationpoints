using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Models;

namespace kCura.IntegrationPoints.Data.Repositories
{
    public interface ISecretsRepository
    {
        Task<string> EncryptAsync(SecretPath secretPath, Dictionary<string, string> secretData);
        Task<Dictionary<string, string>> DecryptAsync(SecretPath secretPath);
        Task DeleteAsync(SecretPath secretPath);
        Task DeleteAllRipSecretsFromAllWorkspacesAsync();
        Task DeleteAllRipSecretsFromWorkspaceAsync(int workspaceID);
        Task DeleteAllRipSecretsFromIntegrationPointAsync(int workspaceID, int integrationPointID);
    }
}