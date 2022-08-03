using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Models;
using kCura.IntegrationPoints.Data.Facades.SecretStore;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class SecretsRepository : ISecretsRepository
    {
        private readonly ISecretStoreFacade _secretStoreFacade;
        private readonly IAPILog _logger;

        public SecretsRepository(ISecretStoreFacade secretStoreFacade, IAPILog apiLog)
        {
            _secretStoreFacade = secretStoreFacade;
            _logger = apiLog.ForContext<SecretsRepository>();
        }

        public async Task<string> EncryptAsync(SecretPath secretPath, Dictionary<string, string> secretData)
        {
            ValidateSecretPath(secretPath);

            var secret = new Secret
            {
                Data = secretData
            };
            await SetSecretAsync(secretPath, secret).ConfigureAwait(false);

            return secretPath.SecretID;
        }

        public async Task<Dictionary<string, string>> DecryptAsync(SecretPath secretPath)
        {
            ValidateSecretPath(secretPath);
            Secret secret = await GetSecretAsync(secretPath).ConfigureAwait(false);
            return secret?.Data;
        }

        public Task DeleteAsync(SecretPath secretPath)
        {
            ValidateSecretPath(secretPath);
            return DeleteSecretAsync(secretPath);
        }

        public Task DeleteAllRipSecretsFromAllWorkspacesAsync()
        {
            SecretPath secretPath = SecretPath.ForAllSecretsInAllWorkspaces();
            return DeleteSecretAsync(secretPath);
        }

        public Task DeleteAllRipSecretsFromWorkspaceAsync(int workspaceID)
        {
            SecretPath secretPath = SecretPath.ForAllSecretsInWorkspace(workspaceID);
            return DeleteSecretAsync(secretPath);
        }

        public Task DeleteAllRipSecretsFromIntegrationPointAsync(int workspaceID, int integrationPointID)
        {
            SecretPath secretPath = SecretPath.ForAllSecretsInIntegrationPoint(
                workspaceID,
                integrationPointID
            );
            return DeleteSecretAsync(secretPath);
        }

        private async Task<Secret> GetSecretAsync(SecretPath secretPath)
        {
            // this try-catch clause was introduced due to an issue with ARMed workspaces (REL-171985)
            // so far, ARM is not capable of copying SQL Secret Catalog records for integration points in workspace database
            // if a secret store entry associated with an integration point is missing, an exception is thrown here
            try
            {
                string secretPathAsString = secretPath.ToString();
                Secret secret = await _secretStoreFacade
                    .GetAsync(secretPathAsString)
                    .ConfigureAwait(false);
                return secret;
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Can not retrieve Secured Configuration for Integration Point. This may be caused by RIP being restored from ARM backup.");
                return null;
            }
        }

        private Task DeleteSecretAsync(SecretPath secretPath)
        {
            return _secretStoreFacade.DeleteAsync(secretPath.ToString());
        }

        private Task SetSecretAsync(SecretPath secretPath, Secret secret)
        {
            return _secretStoreFacade.SetAsync(secretPath.ToString(), secret);
        }

        private void ValidateSecretPath(SecretPath secretPath)
        {
            if (secretPath != null)
            {
                return;
            }
            throw new ArgumentException("Secret path cannot be null");
        }
    }
}