using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Models;
using kCura.IntegrationPoints.Data.QueryOptions;
using kCura.IntegrationPoints.Data.Transformers;
using Relativity.API;
using Relativity.IntegrationPoints.FieldsMapping.Models;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Data.Repositories.Implementations
{
    public class IntegrationPointRepository : IIntegrationPointRepository
    {
        private const string _ERROR_DESERIALIZING_FIELD_MAPPING = "Failed to deserialize field mapping for Integration Point";

        private readonly IRelativityObjectManager _objectManager;
        private readonly IIntegrationPointSerializer _serializer;
        private readonly ISecretsRepository _secretsRepository;
        private readonly IAPILog _logger;

        private readonly Guid _securedConfigurationGuid = IntegrationPointFieldGuids.SecuredConfigurationGuid;

        private readonly int _workspaceID;

        public IntegrationPointRepository(
            IRelativityObjectManager objectManager,
            IIntegrationPointSerializer serializer,
            ISecretsRepository secretsRepository,
            IAPILog apiLog)
        {
            _objectManager = objectManager;
            _serializer = serializer;
            _secretsRepository = secretsRepository;
            _logger = apiLog.ForContext<IntegrationPointRepository>();
            _workspaceID = _objectManager.GetWorkspaceID_Deprecated();
        }

        public Task<IntegrationPoint> ReadAsync(int integrationPointArtifactID)
        {
            return ReadAsync(
                integrationPointArtifactID,
                IntegrationPointQueryOptions.All().Decrypted().WithConfiguration());
        }

        public Task<IntegrationPoint> ReadWithFieldMappingAsync(int integrationPointArtifactID)
        {
            return ReadAsync(
                integrationPointArtifactID,
                IntegrationPointQueryOptions.All().Decrypted().WithFieldMapping().WithConfiguration());
        }

        public Task<IntegrationPoint> ReadEncryptedAsync(int integrationPointArtifactID)
        {
            return ReadAsync(
                integrationPointArtifactID,
                IntegrationPointQueryOptions.All().WithConfiguration());
        }

        public async Task<IEnumerable<FieldMap>> GetFieldMappingAsync(int integrationPointArtifactID)
        {
            IEnumerable<FieldMap> fieldMapping = new List<FieldMap>();

            if (integrationPointArtifactID <= 0)
            {
                return fieldMapping;
            }

            string fieldMappingJson = await GetFieldMappingsAsync(integrationPointArtifactID).ConfigureAwait(false);

            if (string.IsNullOrEmpty(fieldMappingJson))
            {
                return fieldMapping;
            }

            try
            {
                fieldMapping = _serializer.Deserialize<IEnumerable<FieldMap>>(fieldMappingJson);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, _ERROR_DESERIALIZING_FIELD_MAPPING);
                throw;
            }

            return fieldMapping;
        }

        public string GetSecuredConfiguration(int integrationPointArtifactID)
        {
            IntegrationPoint integrationPoint = ReadAsync(
                    integrationPointArtifactID,
                    IntegrationPointQueryOptions.All())
                .GetAwaiter()
                .GetResult();

            return integrationPoint.SecuredConfiguration;
        }

        public string GetName(int integrationPointArtifactID)
        {
            IntegrationPoint integrationPoint = ReadAsync(
                    integrationPointArtifactID,
                    IntegrationPointQueryOptions.All())
                .GetAwaiter()
                .GetResult();

            return integrationPoint.Name;
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

        public void Delete(int integrationPointID)
        {
            _objectManager.Delete(integrationPointID);
        }

        public IList<IntegrationPoint> GetAll(List<int> integrationPointIDs)
        {
            var request = new QueryRequest
            {
                Condition = $"'ArtifactId' in [{string.Join(",", integrationPointIDs)}]"

            };
            return Query(_workspaceID, request);
        }

        public async Task<IList<IntegrationPoint>> GetAllBySourceAndDestinationProviderIDsAsync(int sourceProviderArtifactID, int destinationProviderArtifactID)
        {
            var query = new QueryRequest
            {
                Condition =
                    $"'{IntegrationPointFields.SourceProvider}' == {sourceProviderArtifactID} " +
                    $"AND " +
                    $"'{IntegrationPointFields.DestinationProvider}' == {destinationProviderArtifactID}",
                Fields = new IntegrationPoint().ToFieldList().Where(field => 
                    (field.Guid != IntegrationPointFieldGuids.SourceConfigurationGuid)
                    && (field.Guid != IntegrationPointFieldGuids.DestinationConfigurationGuid)
                    && (field.Guid != IntegrationPointFieldGuids.FieldMappingsGuid))
            };
            IList<IntegrationPoint> integrationPoints = Query(_workspaceID, query);

            foreach (IntegrationPoint integrationPoint in integrationPoints)
            {
                integrationPoint.SourceConfiguration = await GetSourceConfigurationAsync(integrationPoint.ArtifactId).ConfigureAwait(false);
                integrationPoint.DestinationConfiguration = await GetDestinationConfigurationAsync(integrationPoint.ArtifactId).ConfigureAwait(false);
                integrationPoint.FieldMappings = await GetFieldMappingsAsync(integrationPoint.ArtifactId).ConfigureAwait(false);
            }

            return integrationPoints;
        }

        public IList<IntegrationPoint> GetIntegrationPoints(List<int> sourceProviderIds)
        {
            QueryRequest sourceProviderQuery = GetBasicSourceProviderQuery(sourceProviderIds);

            sourceProviderQuery.Fields = new List<FieldRef>
            {
                new FieldRef {Name = IntegrationPointFields.Name}
            };

            return Query(_workspaceID, sourceProviderQuery);
        }

        public IList<IntegrationPoint> GetAllIntegrationPoints()
        {
            var query = new QueryRequest()
            {
                Fields = GetFields()
            };

            return Query(_workspaceID, query);
        }

        public IList<IntegrationPoint> GetIntegrationPointsWithAllFields()
        {
            IList<IntegrationPoint> integrationPointsWithoutFields = GetAllIntegrationPointsWithoutFields();

            return integrationPointsWithoutFields
                .Select(integrationPoint => ReadAsync(integrationPoint.ArtifactId).GetAwaiter().GetResult())
                .ToList();
        }

        private IList<IntegrationPoint> GetAllIntegrationPointsWithoutFields()
        {
            var query = new QueryRequest();

            return Query(_workspaceID, query);
        }

        private IEnumerable<FieldRef> GetFields()
        {
            return BaseRdo.GetFieldMetadata(typeof(IntegrationPoint)).Values.ToList().Select(field => new FieldRef { Guid = field.FieldGuid });
        }

        private QueryRequest GetBasicSourceProviderQuery(List<int> sourceProviderIds)
        {
            return new QueryRequest
            {
                Condition = $"'{IntegrationPointFields.SourceProvider}' in [{string.Join(",", sourceProviderIds)}]"
            };
        }

        private async Task<IntegrationPoint> ReadAsync(int integrationPointArtifactID, IntegrationPointQueryOptions queryOptions)
        {
            IntegrationPoint integrationPoint = _objectManager.Read<IntegrationPoint>(integrationPointArtifactID);

            if (queryOptions.Configuration)
            {
                integrationPoint.SourceConfiguration = await GetSourceConfigurationAsync(integrationPoint.ArtifactId).ConfigureAwait(false);
                integrationPoint.DestinationConfiguration = await GetDestinationConfigurationAsync(integrationPoint.ArtifactId).ConfigureAwait(false);
            }

            if (queryOptions.Decrypt)
            {
                SetDecryptedSecuredConfiguration(_workspaceID, integrationPoint);
            }

            if (queryOptions.FieldMapping)
            {
                integrationPoint.FieldMappings = await GetFieldMappingsAsync(integrationPointArtifactID)
                    .ConfigureAwait(false);
            }

            return integrationPoint;
        }

        private IList<IntegrationPoint> Query(int workspaceID, QueryRequest queryRequest)
        {
            return _objectManager
                .Query<IntegrationPoint>(queryRequest)
                .Select(ip => SetDecryptedSecuredConfiguration(workspaceID, ip))
                .ToList();
        }

        private Task<string> GetFieldMappingsAsync(int integrationPointArtifactID)
        {
            return GetUnicodeLongTextAsync(integrationPointArtifactID, new FieldRef {Guid = IntegrationPointFieldGuids.FieldMappingsGuid});
        }

        private Task<string> GetSourceConfigurationAsync(int integrationPointArtifactID)
        {
            return GetUnicodeLongTextAsync(integrationPointArtifactID, new FieldRef { Guid = IntegrationPointFieldGuids.SourceConfigurationGuid });
        }

        private Task<string> GetDestinationConfigurationAsync(int integrationPointArtifactID)
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

            string secretID = GetSecuredConfiguration(integrationPoint.ArtifactId);

            integrationPoint.SecuredConfiguration = EncryptSecuredConfigurationAsync(
                    secretID,
                    workspaceID,
                    integrationPoint
                )
                .GetAwaiter()
                .GetResult();
        }

        private IntegrationPoint SetDecryptedSecuredConfiguration(int workspaceID, IntegrationPoint rdo)
        {
            string secretID = rdo.GetField<string>(_securedConfigurationGuid);
            if (string.IsNullOrWhiteSpace(secretID))
            {
                return rdo;
            }

            string decryptedSecret = DecryptSecuredConfigurationAsync(workspaceID, rdo)
                .GetAwaiter()
                .GetResult();
            if (string.IsNullOrWhiteSpace(decryptedSecret))
            {
                return rdo;
            }

            rdo.SetField(_securedConfigurationGuid, decryptedSecret);
            return rdo;
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