using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using Newtonsoft.Json;
using Relativity;
using Relativity.API;
using Relativity.Services.Objects.DataContracts;
using Constants = kCura.IntegrationPoints.Core.Constants;

namespace kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations
{
    public class IntegrationPointViewPreLoad : IIntegrationPointViewPreLoad
    {
        private readonly ICaseServiceContext _context;
        private readonly IIntegrationPointBaseFieldsConstants _fieldsConstants;
        private readonly IRelativityProviderConfiguration _relativityProviderSourceConfiguration;
        private readonly IRelativityProviderConfiguration _relativityProviderDestinationConfiguration;
        private readonly IRelativityObjectManager _objectManager;
        private readonly IIntegrationPointRepository _integrationPointRepository;

        private IAPILog _logger;

        public IntegrationPointViewPreLoad(
            ICaseServiceContext context,
            IRelativityProviderConfiguration relativityProviderSourceConfiguration,
            IRelativityProviderConfiguration relativityProviderDestinationConfiguration,
            IIntegrationPointBaseFieldsConstants fieldsConstants,
            IRelativityObjectManager objectManager,
            IIntegrationPointRepository integrationPointRepository,
            IEHHelper helper)
        {
            _context = context;
            _relativityProviderSourceConfiguration = relativityProviderSourceConfiguration;
            _relativityProviderDestinationConfiguration = relativityProviderDestinationConfiguration;
            _fieldsConstants = fieldsConstants;
            _objectManager = objectManager;
            _integrationPointRepository = integrationPointRepository;
            _logger = helper.GetLoggerFactory().GetLogger();
        }

        public void ResetSavedSearch(Action<Artifact> initializeAction, Artifact artifact)
        {
            if (IsRelativityProvider(artifact))
            {
                IDictionary<string, object> sourceConfiguration = GetSourceConfiguration(artifact);

                int savedSearchArtifactId = GetDictionaryValue(sourceConfiguration, "SavedSearchArtifactId");
                int productionId = GetDictionaryValue(sourceConfiguration, "SourceProductionId");
                int sourceViewId = GetDictionaryValue(sourceConfiguration, "SourceViewId");

                if (savedSearchArtifactId == 0 & productionId == 0 & sourceViewId == 0)
                {
                    _logger.LogWarning("savedSearchArtifactId is 0, trying to read it from database Integration Point settings.");
                    int dbSavedSearchArtifactId = GetSavedSearchArtifactId(artifact);
                    sourceConfiguration["SavedSearchArtifactId"] = dbSavedSearchArtifactId;

                    string savedSearchName = GetSavedSearchName(dbSavedSearchArtifactId);
                    sourceConfiguration["SavedSearch"] = savedSearchName;

                    _logger.LogInformation(
                        "PreLoadEventHandler savedSearch configuration reset; savedSearchArtifactId - {savedSearchArtifactId}, savedSearchName - {savedSearchName}.",
                        dbSavedSearchArtifactId,
                        savedSearchName);
                    artifact.Fields[_fieldsConstants.SourceConfiguration].Value.Value = JsonConvert.SerializeObject(sourceConfiguration);
                    initializeAction(artifact);
                }
            }
        }

        public void PreLoad(Artifact artifact)
        {
            if (IsRelativityProvider(artifact))
            {
                IDictionary<string, object> sourceConfiguration = GetSourceConfiguration(artifact);

                _relativityProviderSourceConfiguration.UpdateNames(sourceConfiguration, artifact);
                artifact.Fields[_fieldsConstants.SourceConfiguration].Value.Value = JsonConvert.SerializeObject(sourceConfiguration);
            }

            IDictionary<string, object> destinationConfiguration = GetDestinationConfiguration(artifact);

            _relativityProviderDestinationConfiguration.UpdateNames(destinationConfiguration, artifact);
            artifact.Fields[_fieldsConstants.DestinationConfiguration].Value.Value = JsonConvert.SerializeObject(destinationConfiguration);
        }

        private int GetDictionaryValue(IDictionary<string, object> sourceConfiguration, string key)
        {
            int value = 0;
            if (sourceConfiguration.ContainsKey(key))
            {
                int.TryParse(sourceConfiguration[key].ToString(), out value);
            }

            return value;
        }

        private int GetSavedSearchArtifactId(Artifact artifact)
        {
            string integrationPointName = artifact.Fields[_fieldsConstants.Name].Value.Value.ToString();
            try
            {
                string sourceConfiguration = _integrationPointRepository.GetSourceConfigurationAsync(artifact.ArtifactID)
                    .GetAwaiter()
                    .GetResult();

                Dictionary<string, string> ripSourceConfigurationDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(sourceConfiguration);
                if (!int.TryParse(ripSourceConfigurationDictionary["SavedSearchArtifactId"], out int ripSavedSearchArtifactId))
                {
                    _logger.LogWarning("Could not retrive integer value of SavedSearchArtifactId field in SourceConfiguration for Integration Point ID {id}", artifact.ArtifactID);
                }

                return ripSavedSearchArtifactId;
            }
            catch
            {
                _logger.LogError(
                         "Unable to get SavedSearchArtifactId for integrationPoint - {integrationPoint}",
                         integrationPointName);
                throw;
            }
        }

        private string GetSavedSearchName(int savedSearchArtifactId)
        {
            QueryRequest queryRequest = new QueryRequest
            {
                ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Search },
                Condition = $"'Artifact ID' == {savedSearchArtifactId}",
                Fields = new[] { new FieldRef { Name = "Name" }, new FieldRef { Name = "Owner" } }
            };

            string savedSearch;
            try
            {
                List<RelativityObject> result = _objectManager
                    .QueryAsync(queryRequest)
                    .GetAwaiter()
                    .GetResult();
                savedSearch = result.First().FieldValues
                    .First(x => x.Value.ToString() != string.Empty & x.Value != null)
                    .Value.ToString();
            }
            catch
            {
                _logger.LogError(
                        "ObjectManager unable to read savedSearch with ArtifactId - {savedSearchArtifactId}.",
                        savedSearchArtifactId);
                throw;
            }

            return savedSearch;
        }

        private bool IsRelativityProvider(Artifact artifact)
        {
            int sourceProvider = (int)artifact.Fields[_fieldsConstants.SourceProvider].Value.Value;
            return _context.RelativityObjectManagerService.RelativityObjectManager.Read<SourceProvider>(int.Parse(sourceProvider.ToString())).Name == Constants.IntegrationPoints.RELATIVITY_PROVIDER_NAME;
        }

        private IDictionary<string, object> GetSourceConfiguration(Artifact artifact)
        {
            return GetConfiguration(artifact, _fieldsConstants.SourceConfiguration);
        }

        private IDictionary<string, object> GetDestinationConfiguration(Artifact artifact)
        {
            return GetConfiguration(artifact, _fieldsConstants.DestinationConfiguration);
        }

        private IDictionary<string, object> GetConfiguration(Artifact artifact, string configuration)
        {
            string sourceConfiguration = artifact.Fields[configuration].Value.Value.ToString();
            IDictionary<string, object> settings = new Dictionary<string, object>(
                JsonConvert.DeserializeObject<ExpandoObject>(sourceConfiguration),
                StringComparer.CurrentCultureIgnoreCase);
            return settings;
        }
    }
}
