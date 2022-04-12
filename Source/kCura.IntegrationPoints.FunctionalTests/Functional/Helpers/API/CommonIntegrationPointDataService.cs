using Relativity.IntegrationPoints.Services;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.ChoiceQuery;
using Relativity.Services.Folder;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Search;
using Relativity.Testing.Framework.Api.Kepler;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Choice = Relativity.Services.ChoiceQuery.Choice;

namespace Relativity.IntegrationPoints.Tests.Functional.Helpers.API
{
    internal interface ICommonIntegrationPointDataService
    {
        int WorkspaceId { get; }
        Task<int> GetDestinationProviderIdAsync(string identifier);
        Task<int> GetIntegrationPointTypeByAsync(string name);
        Task<int> GetOverwriteFieldsChoiceIdAsync(string name);
        Task<int> GetRootFolderArtifactIdAsync();
        Task<int> GetSourceProviderIdAsync(string identifier);
        Task<int> GetSavedSearchArtifactIdAsync(string savedSearchName);
        Task<List<FieldMap>> GetIdentifierMappingAsync(int targetWorkspaceId);
    }

    internal class CommonIntegrationPointDataService : ICommonIntegrationPointDataService
    {
        private readonly IKeplerServiceFactory _serviceFactory;
        private readonly int _workspaceId;

        public CommonIntegrationPointDataService(IKeplerServiceFactory serviceFactory, int workspaceId)
        {
            _serviceFactory = serviceFactory;
            _workspaceId = workspaceId;
        }

        public int WorkspaceId => _workspaceId;

        public async Task<int> GetSourceProviderIdAsync(string identifier)
        {
            using (IProviderManager providerManager = _serviceFactory.GetServiceProxy<IProviderManager>())
            {
                return await providerManager.GetSourceProviderArtifactIdAsync(_workspaceId, identifier).ConfigureAwait(false);
            }
        }

        public async Task<int> GetDestinationProviderIdAsync(string identifier)
        {
            using (IProviderManager providerManager = _serviceFactory.GetServiceProxy<IProviderManager>())
            {
                return await providerManager.GetDestinationProviderArtifactIdAsync(_workspaceId, identifier).ConfigureAwait(false);
            }
        }

        public async Task<int> GetIntegrationPointTypeByAsync(string name)
        {
            using (IIntegrationPointTypeManager integrationPointTypeManager = _serviceFactory.GetServiceProxy<IIntegrationPointTypeManager>())
            {
                IList<IntegrationPointTypeModel> types = await integrationPointTypeManager.GetIntegrationPointTypes(_workspaceId).ConfigureAwait(false);

                return types.Single(x => x.Name == name).ArtifactId;
            }
        }

        public async Task<int> GetOverwriteFieldsChoiceIdAsync(string name)
        {
            using (IArtifactGuidManager guidManager = _serviceFactory.GetServiceProxy<IArtifactGuidManager>())
            using (IChoiceQueryManager choiceManager = _serviceFactory.GetServiceProxy<IChoiceQueryManager>())
            {
                int overwriteFieldId = await guidManager.ReadSingleArtifactIdAsync(_workspaceId,
                       Guid.Parse(kCura.IntegrationPoints.Data.IntegrationPointFieldGuids.OverwriteFields))
                   .ConfigureAwait(false);

                List<Choice> choices = await choiceManager.QueryAsync(_workspaceId, overwriteFieldId).ConfigureAwait(false);

                return choices.Single(x => x.Name == name).ArtifactID;
            }
        }

        public async Task<int> GetRootFolderArtifactIdAsync()
        {
            using (IFolderManager folderManager = _serviceFactory.GetServiceProxy<IFolderManager>())
            {
                Folder rootFolder = await folderManager.GetWorkspaceRootAsync(_workspaceId).ConfigureAwait(false);
                
                return rootFolder.ArtifactID;
            }
        }

        public async Task<int> GetSavedSearchArtifactIdAsync(string savedSearchName)
        {
            using (IKeywordSearchManager keywordSearchManager = _serviceFactory.GetServiceProxy<IKeywordSearchManager>())
            {
                Relativity.Services.Query request = new Relativity.Services.Query
                {
                    Condition = $"'Name' == '{savedSearchName}'"
                };
                KeywordSearchQueryResultSet result = await keywordSearchManager.QueryAsync(_workspaceId, request).ConfigureAwait(false);
                return result.Results.First().Artifact.ArtifactID;
            }
        }

        public async Task<List<FieldMap>> GetIdentifierMappingAsync(int targetWorkspaceId)
        {
            using (IObjectManager objectManager = _serviceFactory.GetServiceProxy<IObjectManager>())
            {
                QueryRequest query = PrepareIdentifierFieldsQueryRequest();
                QueryResult sourceQueryResult = await objectManager.QueryAsync(_workspaceId, query, 0, 1).ConfigureAwait(false);
                QueryResult destinationQueryResult = await objectManager.QueryAsync(targetWorkspaceId, query, 0, 1).ConfigureAwait(false);

                return new List<FieldMap>
                {
                    new FieldMap
                    {
                        SourceField = new FieldEntry
                        {
                            DisplayName = sourceQueryResult.Objects.First()["Name"].Value.ToString(),
                            FieldIdentifier = sourceQueryResult.Objects.First().ArtifactID.ToString(),
                            IsIdentifier = true
                        },
                        DestinationField = new FieldEntry
                        {
                            DisplayName = destinationQueryResult.Objects.First()["Name"].Value.ToString(),
                            FieldIdentifier = destinationQueryResult.Objects.First().ArtifactID.ToString(),
                            IsIdentifier = true
                        },
                        FieldMapType = FieldMapType.Identifier
                    }
                };
            }
        }

        private QueryRequest PrepareIdentifierFieldsQueryRequest()
        {
            int fieldArtifactTypeID = (int)ArtifactType.Field;
            QueryRequest queryRequest = new QueryRequest()
            {
                ObjectType = new ObjectTypeRef() { ArtifactTypeID = fieldArtifactTypeID },
                Condition = $"'FieldArtifactTypeID' == {(int)ArtifactType.Document} and 'Is Identifier' == true",
                Fields = new[] { new FieldRef { Name = "Name" } },
                IncludeNameInQueryResult = true
            };
            return queryRequest;
        }
    }
}
