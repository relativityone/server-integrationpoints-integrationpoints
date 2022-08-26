using System;
using System.Collections.Generic;
using System.Dynamic;
using kCura.Apps.Common.Utils.Serializers;
using kCura.EventHandler;
using kCura.IntegrationPoints.Core.Services.ServiceContext;
using kCura.IntegrationPoints.Data;
using Newtonsoft.Json;
using Relativity;
using Relativity.API;
using Relativity.Services.Objects;
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

        public IntegrationPointViewPreLoad(ICaseServiceContext context, IRelativityProviderConfiguration relativityProviderSourceConfiguration,
            IRelativityProviderConfiguration relativityProviderDestinationConfiguration, IIntegrationPointBaseFieldsConstants fieldsConstants)
        {
            _context = context;
            _relativityProviderSourceConfiguration = relativityProviderSourceConfiguration;
            _relativityProviderDestinationConfiguration = relativityProviderDestinationConfiguration;
            _fieldsConstants = fieldsConstants;
        }

        public void PreLoad(Artifact artifact)
        {
            if (IsRelativityProvider(artifact))
            {
                IDictionary<string,object> sourceConfiguration = GetSourceConfiguration(artifact);

                _relativityProviderSourceConfiguration.UpdateNames(sourceConfiguration, artifact);
                artifact.Fields[_fieldsConstants.SourceConfiguration].Value.Value = JsonConvert.SerializeObject(sourceConfiguration);
            }
            IDictionary<string, object> destinationConfiguration = GetDestinationConfiguration(artifact);

            _relativityProviderDestinationConfiguration.UpdateNames(destinationConfiguration, artifact);
            artifact.Fields[_fieldsConstants.DestinationConfiguration].Value.Value = JsonConvert.SerializeObject(destinationConfiguration);
        }

        public void ResetSavedSearchArtifactId(
            Action<Artifact> initializeAction,
            Artifact artifact,
            IEHHelper helper)
        {
            IAPILog logger = helper.GetLoggerFactory().GetLogger();
            int workspaceId = helper.GetActiveCaseID();

            IDictionary<string, object> sourceConfiguration = GetSourceConfiguration(artifact);
            logger.LogWarning("sourceConfiguration - {sourceConfiguration}", sourceConfiguration);
            
            int.TryParse(sourceConfiguration["SavedSearchArtifactId"].ToString(), out int savedSearchArtifactId);
            logger.LogWarning("savedSearchArtifactId - {savedSearchArtifactId}", savedSearchArtifactId);

            if (savedSearchArtifactId == 0)
            {
                string integrationPointName = artifact.Fields[_fieldsConstants.Name].Value.Value.ToString();
                logger.LogWarning("integrationPointName - {integrationPointName}", integrationPointName);
                string ripSourceConfiguration = helper.GetDBContext(workspaceId).ExecuteSqlStatementAsScalar<string>(
                $"SELECT [SourceConfiguration] FROM [EDDS{workspaceId}].[EDDSDBO].[IntegrationPoint] WHERE [Name] = '{integrationPointName}'");
                logger.LogWarning("ripSourceConfiguration - {ripSourceConfiguration}", ripSourceConfiguration);
                Dictionary<string, string> ripSourceConfigurationDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(ripSourceConfiguration);
                logger.LogWarning("ripSavedSearchArtifactId - {SavedSearchArtifactId}", ripSourceConfigurationDictionary["SavedSearchArtifactId"]);
                int.TryParse(ripSourceConfigurationDictionary["SavedSearchArtifactId"], out int ripSavedSearchArtifactId);
                sourceConfiguration["SavedSearchArtifactId"] = ripSavedSearchArtifactId;

                QueryResult result = helper
                    .GetServicesManager()
                    .CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser)
                    .QueryAsync(
                        workspaceId,
                        new QueryRequest
                        {
                            ObjectType = new ObjectTypeRef() { ArtifactTypeID = (int)ArtifactType.Search },
                            Condition = $"'Artifact ID' == {ripSavedSearchArtifactId}",
                            Fields = new[] { new FieldRef { Name = "Name" }, new FieldRef { Name = "Owner" } }
                        },
                        0,
                        1)
                    .GetAwaiter()
                    .GetResult();

                logger.LogWarning("Saved Search Result - {result}", JsonConvert.SerializeObject(result.Objects));

                logger.LogWarning("SourceConfiguration before - {SourceConfiguration}", artifact.Fields[_fieldsConstants.SourceConfiguration].Value.Value);
                artifact.Fields[_fieldsConstants.SourceConfiguration].Value.Value = JsonConvert.SerializeObject(sourceConfiguration);
                logger.LogWarning("SourceConfiguration after - {SourceConfiguration}", artifact.Fields[_fieldsConstants.SourceConfiguration].Value.Value);
                initializeAction(artifact);

            }
        }

        private bool IsRelativityProvider(Artifact artifact)
        {
            int sourceProvider = (int) artifact.Fields[_fieldsConstants.SourceProvider].Value.Value;
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
            IDictionary<string, object> settings = new Dictionary<string, object>(JsonConvert.DeserializeObject<ExpandoObject>(sourceConfiguration), 
                StringComparer.CurrentCultureIgnoreCase);
            return settings;
        }
    }
}