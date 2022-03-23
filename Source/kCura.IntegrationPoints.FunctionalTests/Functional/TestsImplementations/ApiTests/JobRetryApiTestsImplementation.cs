using Relativity.IntegrationPoints.Tests.Functional.Helpers.API;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Models;
using Choice = Relativity.Services.ChoiceQuery.Choice;
using KeywordSearch = Relativity.Testing.Framework.Models.KeywordSearch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using Relativity.Testing.Framework.Api.Services;
using Relativity.IntegrationPoints.Services;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using kCura.IntegrationPoints.Data;
using Relativity.Services.Search;
using Relativity.Services.ChoiceQuery;
using Relativity.Services.ArtifactGuid;
using FluentAssertions;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations.ApiTests
{
    internal class JobRetryApiTestsImplementation
    {
        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;
        private readonly IRipApi ripApi = new RipApi(RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory);
        private const string savedSearchName = "AllDocuments";
        private const string JOB_RETRY_WORKSPACE_NAME = "RIP Job Retry Test";              

        private int savedSearchId;
        private int destinationProviderId;
        private int sourceProviderId;
        private int integrationPointType;
        private int destinationFolderId;
        private int initialSourceWorkspaceDocsCount;
        private int initialDestinationWorkspaceDocsCount;
        private List<Choice> choices;
        private List<FieldMap> fieldsMapping;

        private Workspace destinationWorkspace;
        public Workspace SourceWorkspace => _testsImplementationTestFixture.Workspace;

        public JobRetryApiTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
        {
            _testsImplementationTestFixture = testsImplementationTestFixture;
        }

        public void OnSetUpFixture()
        {
            RelativityFacade.Instance.ImportDocumentsFromCsv(_testsImplementationTestFixture.Workspace, LoadFilesGenerator.GetOrCreateNativesLoadFile(), overwriteMode: DocumentOverwriteMode.AppendOverlay);
            destinationWorkspace = RelativityFacade.Instance.CreateWorkspace(JOB_RETRY_WORKSPACE_NAME, _testsImplementationTestFixture.Workspace.Name);
            CreateSavedSearch(_testsImplementationTestFixture.Workspace.ArtifactID);
            
            RelativityFacade.Instance.ImportDocumentsFromCsv(destinationWorkspace, LoadFilesGenerator.GetOrCreateNativesLoadFile(4), overwriteMode: DocumentOverwriteMode.AppendOverlay);

            initialSourceWorkspaceDocsCount = GetDocumentsFromWorkspace(SourceWorkspace.ArtifactID).Count();
            initialDestinationWorkspaceDocsCount = GetDocumentsFromWorkspace(SourceWorkspace.ArtifactID).Count();
        }

        public void OnTearDownFixture()
        {
            RelativityFacade.Instance.DeleteWorkspace(destinationWorkspace);
        }

        public async Task RunAndRetryIntegrationPoint()
        {
            //1. Job first run:
            //Arrange
            IntegrationPointModel integrationPoint = await PrepareIntegrationPointModel("Overlay").ConfigureAwait(false);
            await ripApi.CreateIntegrationPointAsync(integrationPoint, SourceWorkspace.ArtifactID);
            int expectedItemErrorsToRetry = initialSourceWorkspaceDocsCount - initialDestinationWorkspaceDocsCount;
            string status;
            int transferredItems;
            int itemsWithErrors;

            //Act
            int jobHistoryId = await ripApi.RunIntegrationPointAsync(integrationPoint, SourceWorkspace.ArtifactID);
            GetJobHistoryDetails(jobHistoryId, out status, out transferredItems, out itemsWithErrors);

            //Assert            
            string expectedStatus = "Completed with errors";            

            status.Should().Be(expectedStatus);
            itemsWithErrors.Should().Be(expectedItemErrorsToRetry);

            //2. Job retry:
            //Arrange
            integrationPoint.OverwriteFieldsChoiceId = choices.First(c => c.Name == "Append/Overlay").ArtifactID;

            //Act
            int retryJobHistoryId = await ripApi.RetryIntegrationPointAsync(integrationPoint, SourceWorkspace.ArtifactID);
            GetJobHistoryDetails(retryJobHistoryId, out status, out transferredItems, out itemsWithErrors);

            //Assert
            string expectedRetryStatus = "Completed";

            status.Should().Be(expectedRetryStatus);
            transferredItems.Should().Be(expectedItemErrorsToRetry);
        }

        private void CreateSavedSearch(int workspaceId)
        {
            KeywordSearch keywordSearch = new KeywordSearch { Name = savedSearchName };
            RelativityFacade.Instance.Resolve<IKeywordSearchService>().Require(workspaceId, keywordSearch);
        }

        private async Task<IntegrationPointModel> PrepareIntegrationPointModel(string modeName)
        {
            await GetIntegrationPointsData().ConfigureAwait(false);        

            return new IntegrationPointModel
            {
                SourceConfiguration = GetSourceConfig(),
                DestinationConfiguration = GetDestinationConfiguration(destinationFolderId),                    
                Name = $"{JOB_RETRY_WORKSPACE_NAME} {destinationWorkspace.ArtifactID}",
                FieldMappings = fieldsMapping,
                DestinationProvider = destinationProviderId,
                SourceProvider = sourceProviderId,
                Type = integrationPointType,                
                OverwriteFieldsChoiceId = choices.First(c => c.Name == modeName).ArtifactID           
            };
        }

        private async Task GetIntegrationPointsData()
        {
            ICommonIntegrationPointDataService dataService = new CommonIntegrationPointDataService(RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory, SourceWorkspace.ArtifactID);
            destinationFolderId = await dataService.GetRootFolderArtifactIdAsync().ConfigureAwait(false);
            fieldsMapping = await GetIdentifierMappingAsync(SourceWorkspace.ArtifactID, destinationWorkspace.ArtifactID).ConfigureAwait(false);
            savedSearchId = await GetSavedSearchArtifactIdAsync().ConfigureAwait(false);
            destinationProviderId = await GetDestinationProviderIdAsync().ConfigureAwait(false);
            sourceProviderId = await GetSourceProviderIdAsync().ConfigureAwait(false);
            integrationPointType = await GetIntegrationPointTypeAsync("Export").ConfigureAwait(false);
            choices = await GetChoicesOnFieldAsync(IntegrationPointFieldGuids.OverwriteFieldsGuid).ConfigureAwait(false);
        }

        private RelativityProviderSourceConfiguration GetSourceConfig()
        {
            return new RelativityProviderSourceConfiguration
            {
                TypeOfExport = (int)SourceConfiguration.ExportType.SavedSearch,
                SavedSearchArtifactId = savedSearchId,
                SourceWorkspaceArtifactId = SourceWorkspace.ArtifactID               
            };
        }

        private RelativityProviderDestinationConfiguration GetDestinationConfiguration(int folderId)
        {
            return new RelativityProviderDestinationConfiguration
            {
                CaseArtifactId = SourceWorkspace.ArtifactID,                
                ImportNativeFile = false,
                ArtifactTypeID = (int)ArtifactType.Document,
                DestinationFolderArtifactId = folderId                             
            };
        }

        private async Task<List<FieldMap>> GetIdentifierMappingAsync(int sourceWorkspaceId, int targetWorkspaceId)
        {
            using (IObjectManager objectManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IObjectManager>())
            {
                QueryRequest query = PrepareIdentifierFieldsQueryRequest();
                QueryResult sourceQueryResult = await objectManager.QueryAsync(sourceWorkspaceId, query, 0, 1).ConfigureAwait(false);
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
                ObjectType = new ObjectTypeRef() {ArtifactTypeID = fieldArtifactTypeID},
                Condition = $"'FieldArtifactTypeID' == {(int)ArtifactType.Document} and 'Is Identifier' == true",
                Fields = new[] { new FieldRef { Name = "Name" } },
                IncludeNameInQueryResult = true
            };
            return queryRequest;
        }

        private async Task<int> GetIntegrationPointTypeAsync(string typeName)
        {
            QueryRequest query = new QueryRequest()
            {
                ObjectType = new ObjectTypeRef{Guid = ObjectTypeGuids.IntegrationPointTypeGuid},
                Condition = $"'Name' == '{typeName}'"
            };
            return await GetIdByObjectManagerQueryRun(query);
        }

        private async Task<int> GetSavedSearchArtifactIdAsync()
        {
            using (IKeywordSearchManager keywordSearchManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IKeywordSearchManager>())
            {
                Relativity.Services.Query request = new Relativity.Services.Query
                {
                    Condition = $"'Name' == '{savedSearchName}'"
                };
                KeywordSearchQueryResultSet result = await keywordSearchManager.QueryAsync(SourceWorkspace.ArtifactID, request).ConfigureAwait(false);
                return result.Results.First().Artifact.ArtifactID;
            }
        }

        private async Task<int> GetSourceProviderIdAsync(string identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY)
        {
            QueryRequest query = new QueryRequest
            {
                Condition = $"'{SourceProviderFields.Identifier}' == '{identifier.ToLower()}'",
                ObjectType = new ObjectTypeRef { Guid = ObjectTypeGuids.SourceProviderGuid },
                Fields = new FieldRef[] { new FieldRef { Name = "*" } }
            };

            return await GetIdByObjectManagerQueryRun(query);
        }

        private async Task<int> GetDestinationProviderIdAsync(string identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders.RELATIVITY)
        {
            QueryRequest query = new QueryRequest
            {
                Condition = $"'{DestinationProviderFields.Identifier}' == '{identifier}'",
                ObjectType = new ObjectTypeRef { Guid = ObjectTypeGuids.DestinationProviderGuid }
            };
            return await GetIdByObjectManagerQueryRun(query);
        }

        private async Task<int> GetIdByObjectManagerQueryRun(QueryRequest request)
        {
            using (var objectManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IObjectManager>())
            {
                QueryResult result = await objectManager.QueryAsync(SourceWorkspace.ArtifactID, request, 0, 1).ConfigureAwait(false);
                return result.Objects.Single().ArtifactID;
            }
        }

        private async Task<List<Choice>> GetChoicesOnFieldAsync(Guid fieldGuid)
        {
            int fieldId = await ReadFieldIdByGuidAsync(SourceWorkspace.ArtifactID, fieldGuid).ConfigureAwait(false);
            using (IChoiceQueryManager choiceManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IChoiceQueryManager>())
            {
                return await choiceManager.QueryAsync(SourceWorkspace.ArtifactID, fieldId).ConfigureAwait(false);
            }
        }

        private async Task<int> ReadFieldIdByGuidAsync(int workspaceArtifactId, Guid fieldGuid)
        {
            using (IArtifactGuidManager guidManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IArtifactGuidManager>())
            {
                return await guidManager.ReadSingleArtifactIdAsync(workspaceArtifactId, fieldGuid).ConfigureAwait(false);
            }
        }

        private List<RelativityObject> GetDocumentsFromWorkspace(int workspaceId)
        {
            using (IObjectManager objectManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IObjectManager>())
            {
                QueryRequest request = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Document },
                    Fields = new FieldRef[] { new FieldRef { Name = "*" } }
                };
                return objectManager.QueryAsync(workspaceId, request, 0, int.MaxValue).GetAwaiter().GetResult().Objects.ToList();
            }
        }

        private void GetJobHistoryDetails(int jobHistoryId, out string status, out int transferredItems, out int itemsWithError)
        {
            RelativityObject jobHistoryDetails = GetJobHistoryByObjectManager(jobHistoryId);
            status = ((IList<RelativityObjectValue>)jobHistoryDetails.FieldValues.Where(f => f.Field.Name == "Job Status").FirstOrDefault().Value).Single().Name;

            string itemsTransferredFieldValue = ((IList<RelativityObjectValue>)jobHistoryDetails.FieldValues.Where(f => f.Field.Name == "Items Transferred").FirstOrDefault().Value).Single().Name;
            int.TryParse(itemsTransferredFieldValue, out transferredItems);

            string itemsWithErrorsFieldValue = ((IList<RelativityObjectValue>)jobHistoryDetails.FieldValues.Where(f => f.Field.Name == "Items with Errors").FirstOrDefault().Value).Single().Name;
            int.TryParse(itemsWithErrorsFieldValue, out itemsWithError);
        }

        private RelativityObject GetJobHistoryByObjectManager(int jobHistoryId)
        {
            using (IObjectManager objectManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IObjectManager>())
            {
                QueryRequest request = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { Guid = ObjectTypeGuids.JobHistoryGuid },
                    Fields = new FieldRef[]
                    {
                        new FieldRef { Name = "Job Status" },
                        new FieldRef { Name = "Items Transferred" },
                        new FieldRef { Name = "Items with Errors" }
                    },
                    Condition = $"'ArtifactId' == '{jobHistoryId}'"
                };

                return objectManager.QueryAsync(SourceWorkspace.ArtifactID, request, 0, int.MaxValue)
                    .GetAwaiter().GetResult().Objects.ToList().FirstOrDefault();            
            }
        }

    }
}
