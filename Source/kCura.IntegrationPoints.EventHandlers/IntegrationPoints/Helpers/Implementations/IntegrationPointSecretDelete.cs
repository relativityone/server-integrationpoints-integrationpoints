using kCura.IntegrationPoints.Data.Repositories;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
    public class IntegrationPointSecretDelete : IIntegrationPointSecretDelete
    {
        private readonly int _workspaceID;
        private readonly IIntegrationPointRepository _integrationPointRepository;
        private readonly ISecretsRepository _secretsRepository;

        public IntegrationPointSecretDelete(
            int workspaceID,
            ISecretsRepository secretsRepository, 
            IIntegrationPointRepository integrationPointRepository)
        {
            _workspaceID = workspaceID;
            _secretsRepository = secretsRepository;
            _integrationPointRepository = integrationPointRepository;
        }

        public void DeleteSecret(int integrationPointId)
        {
            string integrationPointSecret = _integrationPointRepository
                .GetSecuredConfiguration(integrationPointId);
            //Old IntegrationPoints don't contain SecuredConfiguration
            if (!string.IsNullOrWhiteSpace(integrationPointSecret))
            {
                _secretsRepository.DeleteAllRipSecretsFromIntegrationPointAsync(
                        _workspaceID, 
                        integrationPointId
                    ).GetAwaiter()
                    .GetResult();
            }
        }
        
    }
}