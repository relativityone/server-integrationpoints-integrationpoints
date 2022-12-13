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

        public async Task<string> GetFieldMappingAsync(int integrationPointArtifactID)
        {
            return await GetUnicodeLongTextAsync(integrationPointArtifactID, new FieldRef { Guid = IntegrationPointFieldGuids.FieldMappingsGuid }).ConfigureAwait(false);
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

            string decryptedSecuredConfiguration = integrationPoint.SecuredConfiguration;
            SetEncryptedSecuredConfiguration(_workspaceID, integrationPoint);
            _objectManager.Update(integrationPoint);

            integrationPoint.SecuredConfiguration = decryptedSecuredConfiguration;
            return integrationPoint.ArtifactId;
        }

        public void UpdateType(int artifactId, int? type)
        {
            List<FieldRefValuePair> fieldValues = new List<FieldRefValuePair>
            {
                new FieldRefValuePair
                {
                    Field = new FieldRef { Guid = IntegrationPointFieldGuids.TypeGuid },
                    Value = type,
                },
            };

            _objectManager.Update(artifactId, fieldValues);
        }

        public void UpdateHasErrors(int artifactId, bool hasErrors)
        {
            List<FieldRefValuePair> fieldValues = new List<FieldRefValuePair>
            {
                new FieldRefValuePair
                {
                    Field = new FieldRef { Guid = IntegrationPointFieldGuids.HasErrorsGuid },
                    Value = hasErrors,
                },
            };

            _objectManager.Update(artifactId, fieldValues);
        }

        public void UpdateLastAndNextRunTime(int artifactId, DateTime? lastRuntime, DateTime? nextRuntime)
        {
            List<FieldRefValuePair> fieldValues = new List<FieldRefValuePair>
            {
                new FieldRefValuePair
                {
                    Field = new FieldRef { Guid = IntegrationPointFieldGuids.LastRuntimeUTCGuid },
                    Value = lastRuntime,
                },
                new FieldRefValuePair
                {
                    Field = new FieldRef { Guid = IntegrationPointFieldGuids.NextScheduledRuntimeUTCGuid },
                    Value = nextRuntime,
                },
            };

            _objectManager.Update(artifactId, fieldValues);
        }

        public void DisableScheduler(int artifactId)
        {
            List<FieldRefValuePair> fieldValues = new List<FieldRefValuePair>
            {
                new FieldRefValuePair
                {
                    Field = new FieldRef { Guid = IntegrationPointFieldGuids.ScheduleRuleGuid },
                    Value = null,
                },
                new FieldRefValuePair
                {
                    Field = new FieldRef { Guid = IntegrationPointFieldGuids.NextScheduledRuntimeUTCGuid },
                    Value = null,
                },
                new FieldRefValuePair
                {
                    Field = new FieldRef { Guid = IntegrationPointFieldGuids.EnableSchedulerGuid },
                    Value = false,
                },
            };

            _objectManager.Update(artifactId, fieldValues);
        }

        public void UpdateJobHistory(int artifactId, List<int> jobHistory)
        {
            List<FieldRefValuePair> fieldValues = new List<FieldRefValuePair>
            {
                new FieldRefValuePair
                {
                    Field = new FieldRef { Guid = IntegrationPointFieldGuids.JobHistoryGuid },
                    Value = jobHistory?.ToArray(),
                },
            };

            _objectManager.Update(artifactId, fieldValues);
        }

        public void UpdateSourceConfiguration(int artifactId, string sourceConfiguration)
        {
            List<FieldRefValuePair> fieldValues = new List<FieldRefValuePair>
            {
                new FieldRefValuePair
                {
                    Field = new FieldRef { Guid = IntegrationPointFieldGuids.SourceConfigurationGuid },
                    Value = sourceConfiguration,
                },
            };

            _objectManager.Update(artifactId, fieldValues);
        }

        public void UpdateDestinationConfiguration(int artifactId, string destinationConfiguration)
        {
            List<FieldRefValuePair> fieldValues = new List<FieldRefValuePair>
            {
                new FieldRefValuePair
                {
                    Field = new FieldRef { Guid = IntegrationPointFieldGuids.DestinationConfigurationGuid },
                    Value = destinationConfiguration,
                },
            };

            _objectManager.Update(artifactId, fieldValues);
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
                string decryptedConfiguration = await DecryptSecuredConfigurationAsync(_workspaceID, integrationPoint).ConfigureAwait(false);
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
