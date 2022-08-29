﻿using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
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

        public IntegrationPointViewPreLoad(
            ICaseServiceContext context,
            IRelativityProviderConfiguration relativityProviderSourceConfiguration,
            IRelativityProviderConfiguration relativityProviderDestinationConfiguration,
            IIntegrationPointBaseFieldsConstants fieldsConstants)
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
                IDictionary<string, object> sourceConfiguration = GetSourceConfiguration(artifact);

                _relativityProviderSourceConfiguration.UpdateNames(sourceConfiguration, artifact);
                artifact.Fields[_fieldsConstants.SourceConfiguration].Value.Value = JsonConvert.SerializeObject(sourceConfiguration);
            }

            IDictionary<string, object> destinationConfiguration = GetDestinationConfiguration(artifact);

            _relativityProviderDestinationConfiguration.UpdateNames(destinationConfiguration, artifact);
            artifact.Fields[_fieldsConstants.DestinationConfiguration].Value.Value = JsonConvert.SerializeObject(destinationConfiguration);
        }

        public void ResetSavedSearch(
            Action<Artifact> initializeAction,
            Artifact artifact,
            IEHHelper helper)
        {
            IDictionary<string, object> sourceConfiguration = GetSourceConfiguration(artifact);
            int.TryParse(sourceConfiguration["SavedSearchArtifactId"].ToString(), out int savedSearchArtifactId);

            if (savedSearchArtifactId == 0)
            {
                IAPILog logger = helper.GetLoggerFactory().GetLogger();
                int workspaceId = helper.GetActiveCaseID();
                logger.LogWarning("savedSearchArtifactId is 0, trying to retrieve from database");

                int dbSavedSearchArtifactId = GetSavedSearchArtifactId(artifact, helper, workspaceId);
                sourceConfiguration["SavedSearchArtifactId"] = dbSavedSearchArtifactId;

                string savedSearchName = GetSavedSearchName(helper, workspaceId, dbSavedSearchArtifactId);
                sourceConfiguration["SavedSearch"] = savedSearchName;

                logger.LogInformation(
                    "PreLoadEventHandler savedSearch configuration reset; savedSearchArtifactId - {savedSearchArtifactId}, savedSearchName - {savedSearchName}",
                    dbSavedSearchArtifactId,
                    savedSearchName);
                artifact.Fields[_fieldsConstants.SourceConfiguration].Value.Value = JsonConvert.SerializeObject(sourceConfiguration);
                initializeAction(artifact);
            }
        }

        private int GetSavedSearchArtifactId(Artifact artifact, IEHHelper helper, int workspaceId)
        {
            string integrationPointName = artifact.Fields[_fieldsConstants.Name].Value.Value.ToString();
            string sqlQuery = $"SELECT [SourceConfiguration] FROM [EDDS{workspaceId}].[EDDSDBO].[IntegrationPoint] WHERE [Name] = '{integrationPointName}'";
            string dbSourceConfiguration = helper.GetDBContext(workspaceId).ExecuteSqlStatementAsScalar<string>(sqlQuery);
            Dictionary<string, string> ripSourceConfigurationDictionary = JsonConvert.DeserializeObject<Dictionary<string, string>>(dbSourceConfiguration);
            int.TryParse(ripSourceConfigurationDictionary["SavedSearchArtifactId"], out int ripSavedSearchArtifactId);
            return ripSavedSearchArtifactId;
        }

        private string GetSavedSearchName(IEHHelper helper, int workspaceId, int savedSearchArtifactId)
        {
            QueryRequest queryRequest = new QueryRequest
            {
                ObjectType = new ObjectTypeRef() { ArtifactTypeID = (int)ArtifactType.Search },
                Condition = $"'Artifact ID' == {savedSearchArtifactId}",
                Fields = new[] { new FieldRef { Name = "Name" }, new FieldRef { Name = "Owner" } }
            };
            QueryResult result = helper
                .GetServicesManager()
                .CreateProxy<IObjectManager>(ExecutionIdentity.CurrentUser)
                .QueryAsync(workspaceId, queryRequest, 0, 1)
                .GetAwaiter()
                .GetResult();

            string savedSearch = result.Objects[0].FieldValues.First(x => x.Value.ToString() != string.Empty & x.Value != null)
                .Value.ToString();
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
