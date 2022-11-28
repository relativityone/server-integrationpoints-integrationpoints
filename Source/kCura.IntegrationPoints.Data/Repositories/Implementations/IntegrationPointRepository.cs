using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Models;
using kCura.IntegrationPoints.Data.Transformers;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class IntegrationPointRepository : IIntegrationPointRepository
    {
        private readonly IRelativityObjectManager _objectManager;
        private readonly ISecretsRepository _secretsRepository;
        private readonly IAPILog _logger;

        private readonly Guid _securedConfigurationGuid = IntegrationPointFieldGuids.SecuredConfigurationGuid;

        private readonly int _workspaceID;

        public IntegrationPointRepository(
            IRelativityObjectManager objectManager,
            ISecretsRepository secretsRepository,
            IAPILog apiLog)
        {
            _objectManager = objectManager;
            _secretsRepository = secretsRepository;
            _logger = apiLog.ForContext<IntegrationPointRepository>();
            _workspaceID = _objectManager.GetWorkspaceID_Deprecated();
        }

        public Task<IntegrationPoint> ReadAsync(int integrationPointArtifactID)
        {
            return ReadAsync(integrationPointArtifactID, true);
        }

        public Task<IntegrationPoint> ReadEncryptedAsync(int integrationPointArtifactID)
        {
            return ReadAsync(integrationPointArtifactID, false);
        }

        public async Task<string> GetFieldMappingAsync(int integrationPointArtifactID)
        {
            return await GetUnicodeLongTextAsync(integrationPointArtifactID, new FieldRef { Guid = IntegrationPointFieldGuids.FieldMappingsGuid });
        }

        public string GetEncryptedSecuredConfiguration(int integrationPointArtifactID)
        {
            IntegrationPoint integrationPoint = ReadAsync(
                    integrationPointArtifactID,
                    false)
                .GetAwaiter()
                .GetResult();

            return integrationPoint.SecuredConfiguration;
        }

        public string GetName(int integrationPointArtifactID)
        {
            IntegrationPoint integrationPoint = ReadAsync(integrationPointArtifactID, false)
                .GetAwaiter()
                .GetResult();

            return integrationPoint.Name;
        }

        public List<IntegrationPoint> ReadAll()
        {
            var integrationPointsWithoutFields = _objectManager.Query<IntegrationPoint>(new QueryRequest()).ToList();

            return integrationPointsWithoutFields
                .Select(integrationPoint => ReadAsync(integrationPoint.ArtifactId, false).GetAwaiter().GetResult())
                .ToList();
        }

        public List<IntegrationPoint> ReadAllByIds(List<int> integrationPointIDs)
        {
            var request = new QueryRequest
            {
                Condition = $"'ArtifactId' in [{string.Join(",", integrationPointIDs)}]"
            };
            return _objectManager.Query<IntegrationPoint>(request).ToList();
        }

        public async Task<List<IntegrationPoint>> ReadBySourceAndDestinationProviderAsync(int sourceProviderArtifactID, int destinationProviderArtifactID)
        {
            var query = new QueryRequest
            {
                Condition =
                    $"'{IntegrationPointFields.SourceProvider}' == {sourceProviderArtifactID} " +
                    $"AND " +
                    $"'{IntegrationPointFields.DestinationProvider}' == {destinationProviderArtifactID}",
                Fields = RDOConverter.GetFieldList<IntegrationPoint>().Where(field =>
                    (field.Guid != IntegrationPointFieldGuids.SourceConfigurationGuid)
                    && (field.Guid != IntegrationPointFieldGuids.DestinationConfigurationGuid)
                    && (field.Guid != IntegrationPointFieldGuids.FieldMappingsGuid))
            };
            List<IntegrationPoint> integrationPoints = _objectManager.Query<IntegrationPoint>(query).ToList();

            foreach (IntegrationPoint integrationPoint in integrationPoints)
            {
                integrationPoint.SourceConfiguration = await GetSourceConfigurationAsync(integrationPoint.ArtifactId).ConfigureAwait(false);
                integrationPoint.DestinationConfiguration = await GetDestinationConfigurationAsync(integrationPoint.ArtifactId).ConfigureAwait(false);
                integrationPoint.FieldMappings = await GetFieldMappingAsync(integrationPoint.ArtifactId).ConfigureAwait(false);
            }

            return integrationPoints;
        }

        public List<IntegrationPoint> ReadBySourceProviders(List<int> sourceProviderIds)
        {
            QueryRequest sourceProviderQuery = GetBasicSourceProviderQuery(sourceProviderIds);
            sourceProviderQuery.Fields = new List<FieldRef>
            {
                new FieldRef { Name = IntegrationPointFields.Name }
            };

            return _objectManager.Query<IntegrationPoint>(sourceProviderQuery).ToList();
        }

        public int CreateOrUpdate(IntegrationPoint integrationPoint)
        {
            if (integrationPoint.ArtifactId <= 0)
            {
                var emptyIntegrationPoint = new IntegrationPoint();
                integrationPoint.ArtifactId = _objectManager.Create(emptyIntegrationPoint);
            }

            Update(integrationPoint);
            return integrationPoint.ArtifactId;
        }

        public void Update(IntegrationPoint integrationPoint)
        {
            string decryptedSecuredConfiguration = integrationPoint.SecuredConfiguration;
            SetEncryptedSecuredConfiguration(_workspaceID, integrationPoint);
            _objectManager.Update(integrationPoint);
            integrationPoint.SecuredConfiguration = decryptedSecuredConfiguration;
        }

        // This method is used by JobHistoryErrorService.
        // It should be placed in IntegrationPointService but this will cause circular dependencies. We need to clean up dependencies hell first and then move this method to the right place.
        public void UpdateHasErrors(int integrationPointArtifactId, bool hasErrors)
        {
            IntegrationPoint integrationPoint = _objectManager.Read<IntegrationPoint>(integrationPointArtifactId);
            integrationPoint.HasErrors = hasErrors;
            _objectManager.Update(integrationPoint);
        }

        public void Delete(int integrationPointID)
        {
            _objectManager.Delete(integrationPointID);
        }

        private QueryRequest GetBasicSourceProviderQuery(List<int> sourceProviderIds)
        {
            return new QueryRequest
            {
                Condition = $"'{IntegrationPointFields.SourceProvider}' in [{string.Join(",", sourceProviderIds)}]"
            };
        }

        private async Task<IntegrationPoint> ReadAsync(int integrationPointArtifactID, bool decryptSecuredConfiguration)
        {
            IntegrationPoint integrationPoint = _objectManager.Read<IntegrationPoint>(integrationPointArtifactID);

            if (decryptSecuredConfiguration)
            {
                string decryptedConfiguration = await DecryptSecuredConfigurationAsync(_workspaceID, integrationPoint);
                integrationPoint.SecuredConfiguration = decryptedConfiguration ?? integrationPoint.SecuredConfiguration;
            }

            return integrationPoint;
        }

        public Task<string> GetSourceConfigurationAsync(int integrationPointArtifactID)
        {
            return GetUnicodeLongTextAsync(integrationPointArtifactID, new FieldRef { Guid = IntegrationPointFieldGuids.SourceConfigurationGuid });
        }

        public Task<string> GetDestinationConfigurationAsync(int integrationPointArtifactID)
        {
            return GetUnicodeLongTextAsync(integrationPointArtifactID, new FieldRef { Guid = IntegrationPointFieldGuids.DestinationConfigurationGuid });
        }

        private async Task<string> GetUnicodeLongTextAsync(int integrationPointArtifactID, FieldRef field)
        {
            Stream unicodeLongTextStream = _objectManager.StreamUnicodeLongText(integrationPointArtifactID, field);
            using (StreamReader unicodeLongTextStreamReader = new StreamReader(unicodeLongTextStream))
            {
                return await unicodeLongTextStreamReader.ReadToEndAsync().ConfigureAwait(false);
            }
        }

        private void SetEncryptedSecuredConfiguration(int workspaceID, IntegrationPoint integrationPoint)
        {
            FieldRefValuePair securedConfigurationField = integrationPoint
                .ToFieldValues()
                .FirstOrDefault(x => x.Field.Guid == _securedConfigurationGuid);

            if (securedConfigurationField == null)
            {
                return;
            }

            string secretID = GetEncryptedSecuredConfiguration(integrationPoint.ArtifactId);

            integrationPoint.SecuredConfiguration = EncryptSecuredConfigurationAsync(
                    secretID,
                    workspaceID,
                    integrationPoint
                )
                .GetAwaiter()
                .GetResult();
        }

        private async Task<string> EncryptSecuredConfigurationAsync(
            string secretID,
            int workspaceID,
            IntegrationPoint integrationPoint)
        {
            if (integrationPoint.SecuredConfiguration == null)
            {
                return null;
            }
            try
            {
                SecretPath secretPath = GetSecretPathOrGenerateNewOne(
                    workspaceID,
                    integrationPoint.ArtifactId,
                    secretID
                );
                Dictionary<string, string> secretData = CreateSecuredConfigurationSecretData(
                    integrationPoint.SecuredConfiguration
                );
                return await _secretsRepository
                    .EncryptAsync(secretPath, secretData)
                    .ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Can not write Secured Configuration for Integration Point record during encryption process.");
                //Ignore as Integration Point RDO doesn't always include SecuredConfiguration
                //Any access to missing fieldGuid will throw FieldNotFoundException
                return integrationPoint.SecuredConfiguration;
            }
        }

        private async Task<string> DecryptSecuredConfigurationAsync(int workspaceID, IntegrationPoint integrationPoint)
        {
            string secretID = integrationPoint.SecuredConfiguration;
            if (string.IsNullOrWhiteSpace(secretID))
            {
                return null;
            }

            SecretPath secretPath = GetSecretPathOrGenerateNewOne(
                workspaceID,
                integrationPoint.ArtifactId,
                secretID
            );
            Dictionary<string, string> secretData = await _secretsRepository
                .DecryptAsync(secretPath)
                .ConfigureAwait(false);
            return ReadSecuredConfigurationFromSecretData(secretData);
        }

        private Dictionary<string, string> CreateSecuredConfigurationSecretData(string securedConfiguration)
        {
            return new Dictionary<string, string>
            {
                [nameof(IntegrationPoint.SecuredConfiguration)] = securedConfiguration
            };
        }

        private SecretPath GetSecretPathOrGenerateNewOne(int workspaceID, int integrationPointID, string secretID)
        {
            if (string.IsNullOrWhiteSpace(secretID))
            {
                secretID = Guid.NewGuid().ToString();
            }

            return SecretPath.ForIntegrationPointSecret(
                workspaceID,
                integrationPointID,
                secretID
            );
        }

        private string ReadSecuredConfigurationFromSecretData(Dictionary<string, string> secretData)
        {
            return secretData?[nameof(IntegrationPoint.SecuredConfiguration)];
        }
    }
}
