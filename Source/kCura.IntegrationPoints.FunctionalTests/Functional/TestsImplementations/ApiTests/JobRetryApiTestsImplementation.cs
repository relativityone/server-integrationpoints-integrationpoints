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
using kCura.IntegrationPoints.Synchronizers.RDO;
using static kCura.IntegrationPoints.Core.Constants.IntegrationPoints;
using Relativity.IntegrationPoints.Tests.Functional.DataModels;
using kCura.IntegrationPoints.Domain.Models;
using Relativity.Services.Folder;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations.ApiTests
{
    internal class JobRetryApiTestsImplementation
    {
        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;
        private readonly IRipApi ripApi = new RipApi(RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory);
        private const string savedSearchName = "AllDocuments";
        private const string JOB_RETRY_WORKSPACE_NAME = "RIP Job Retry Test";

        ICommonIntegrationPointDataService sourceDataService;
        ICommonIntegrationPointDataService destinationDataService;

        private int savedSearchId;
        private int destinationProviderId;
        private int sourceProviderId;
        private int integrationPointType;
        private int destinationFolderId;
        //private int overwriteFieldsChoiceId;       
        //private List<Choice> choices;
        private List<FieldMap> fieldsMapping;

        private int initialSourceWorkspaceDocsCount;
        private int initialDestinationWorkspaceDocsCount;
        private Workspace destinationWorkspace;
        public Workspace SourceWorkspace;

        public JobRetryApiTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
        {
            _testsImplementationTestFixture = testsImplementationTestFixture;
        }

        public void OnSetUpFixture()
        {
            SourceWorkspace = _testsImplementationTestFixture.Workspace;
            RelativityFacade.Instance.ImportDocumentsFromCsv(_testsImplementationTestFixture.Workspace, LoadFilesGenerator.GetOrCreateNativesLoadFile(), overwriteMode: DocumentOverwriteMode.AppendOverlay);
            destinationWorkspace = RelativityFacade.Instance.CreateWorkspace(JOB_RETRY_WORKSPACE_NAME, _testsImplementationTestFixture.Workspace.Name);
            CreateSavedSearch(_testsImplementationTestFixture.Workspace.ArtifactID);

            RelativityFacade.Instance.ImportDocumentsFromCsv(destinationWorkspace, LoadFilesGenerator.CreateNativesLoadFileWithLimitedItems(4), overwriteMode: DocumentOverwriteMode.AppendOverlay);

            initialSourceWorkspaceDocsCount = GetDocumentsFromWorkspace(SourceWorkspace.ArtifactID).Count();
            initialDestinationWorkspaceDocsCount = GetDocumentsFromWorkspace(destinationWorkspace.ArtifactID).Count();

            sourceDataService = new CommonIntegrationPointDataService(RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory, SourceWorkspace.ArtifactID);
            destinationDataService = new CommonIntegrationPointDataService(RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory, destinationWorkspace.ArtifactID);
        }

        public void OnTearDownFixture()
        {
            RelativityFacade.Instance.DeleteWorkspace(destinationWorkspace);
        }

        public async Task RunAndRetryIntegrationPoint()
        {
            //1. Job first run:
            //Arrange
            IntegrationPointModel integrationPoint = await PrepareIntegrationPointModel().ConfigureAwait(false);
            await ripApi.CreateIntegrationPointAsync(integrationPoint, SourceWorkspace.ArtifactID);
            int expectedItemErrorsToRetry = initialSourceWorkspaceDocsCount - initialDestinationWorkspaceDocsCount;
            string expectedStatus = "Completed with errors";
            int transferredItems;
            int itemsWithErrors;

            //Act
            int jobHistoryId = await ripApi.RunIntegrationPointAsync(integrationPoint, SourceWorkspace.ArtifactID);

            //Assert             
            Func<Task> endRun = async () => { await ripApi.WaitForJobToFinishAsync(jobHistoryId, SourceWorkspace.ArtifactID, checkDelayInMs: 500, expectedStatus); };
            endRun.ShouldNotThrow();
            GetJobHistoryDetails(jobHistoryId, out transferredItems, out itemsWithErrors);
            itemsWithErrors.Should().Be(expectedItemErrorsToRetry);

            //2. Job retry:
            //Arrange
            integrationPoint.OverwriteFieldsChoiceId = await GetProperOverlayBehaviorForIntegrationPoint(sourceDataService, "Append/Overlay");
            expectedStatus = "Completed";

            //Act
            int retryJobHistoryId = await ripApi.RetryIntegrationPointAsync(integrationPoint, SourceWorkspace.ArtifactID);

            //Assert
            Func<Task> endRetry = async () => { await ripApi.WaitForJobToFinishAsync(retryJobHistoryId, SourceWorkspace.ArtifactID, checkDelayInMs: 500, expectedStatus); };
            endRetry.ShouldNotThrow();
            GetJobHistoryDetails(retryJobHistoryId, out transferredItems, out itemsWithErrors);
            transferredItems.Should().Be(expectedItemErrorsToRetry);
        }

        private void CreateSavedSearch(int workspaceId)
        {
            KeywordSearch keywordSearch = new KeywordSearch { Name = savedSearchName };
            RelativityFacade.Instance.Resolve<IKeywordSearchService>().Require(workspaceId, keywordSearch);
        }

        private async Task<IntegrationPointModel> PrepareIntegrationPointModel()
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
                OverwriteFieldsChoiceId = await GetProperOverlayBehaviorForIntegrationPoint(sourceDataService, "Overlay Only"),
                EmailNotificationRecipients = "",
                ScheduleRule = new ScheduleModel(),
                LogErrors = true
            };
        }

        private async Task GetIntegrationPointsData()
        {
            //overwriteFieldsChoiceId = await GetProperOverlayBehaviorForIntegrationPoint(sourceDataService, "Overlay Only");
            integrationPointType = await sourceDataService.GetIntegrationPointTypeByAsync(IntegrationPointTypes.ExportName).ConfigureAwait(false);
            destinationFolderId = await destinationDataService.GetRootFolderArtifactIdAsync().ConfigureAwait(false);
            fieldsMapping = await GetIdentifierMappingAsync(SourceWorkspace.ArtifactID, destinationWorkspace.ArtifactID).ConfigureAwait(false);
            savedSearchId = await GetSavedSearchArtifactIdAsync().ConfigureAwait(false);
            destinationProviderId = await sourceDataService.GetDestinationProviderIdAsync(DestinationProviders.RELATIVITY).ConfigureAwait(false);
            sourceProviderId = await sourceDataService.GetSourceProviderIdAsync(SourceProviders.RELATIVITY).ConfigureAwait(false);

            //choices = await GetChoicesOnFieldAsync(IntegrationPointFieldGuids.OverwriteFieldsGuid).ConfigureAwait(false);
        }

        private RelativityProviderSourceConfiguration GetSourceConfig()
        {
            return new RelativityProviderSourceConfiguration
            {
                TypeOfExport = (int)SourceConfiguration.ExportType.SavedSearch,
                SavedSearchArtifactId = savedSearchId,
                SourceWorkspaceArtifactId = SourceWorkspace.ArtifactID,
                UseDynamicFolderPath = false
            };
        }

        private RelativityProviderDestinationConfiguration GetDestinationConfiguration(int folderId)
        {
            return new RelativityProviderDestinationConfiguration
            {
                CaseArtifactId = destinationWorkspace.ArtifactID,
                FieldOverlayBehavior = RelativityProviderValidationMessages.FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_DEFAULT,
                ImportNativeFile = false,
                ArtifactTypeID = (int)ArtifactType.Document,
                DestinationFolderArtifactId = folderId,
                FolderPathSourceField = 0,
                UseFolderPathInformation = false
            };
        }

        private async Task<int> GetProperOverlayBehaviorForIntegrationPoint(ICommonIntegrationPointDataService dataService, string overlayBehaviorName)
        {
            return await dataService.GetOverwriteFieldsChoiceIdAsync(overlayBehaviorName).ConfigureAwait(false);
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
                ObjectType = new ObjectTypeRef() { ArtifactTypeID = fieldArtifactTypeID },
                Condition = $"'FieldArtifactTypeID' == {(int)ArtifactType.Document} and 'Is Identifier' == true",
                Fields = new[] { new FieldRef { Name = "Name" } },
                IncludeNameInQueryResult = true
            };
            return queryRequest;
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

        private async Task<int> GetSourceProviderIdAsync(string identifier = SourceProviders.RELATIVITY)
        {
            QueryRequest query = new QueryRequest
            {
                Condition = $"'{SourceProviderFields.Identifier}' == '{identifier.ToLower()}'",
                ObjectType = new ObjectTypeRef { Guid = ObjectTypeGuids.SourceProviderGuid },
                Fields = new FieldRef[] { new FieldRef { Name = "*" } }
            };

            return await GetIdByObjectManagerQueryRun(query);
        }

        private async Task<int> GetDestinationProviderIdAsync(string identifier = DestinationProviders.RELATIVITY)
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

        //private async Task<List<Choice>> GetChoicesOnFieldAsync(Guid fieldGuid)
        //{
        //    int fieldId = await ReadFieldIdByGuidAsync(SourceWorkspace.ArtifactID, fieldGuid).ConfigureAwait(false);
        //    using (IChoiceQueryManager choiceManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IChoiceQueryManager>())
        //    {
        //        return await choiceManager.QueryAsync(SourceWorkspace.ArtifactID, fieldId).ConfigureAwait(false);
        //    }
        //}

        //private async Task<int> ReadFieldIdByGuidAsync(int workspaceArtifactId, Guid fieldGuid)
        //{
        //    using (IArtifactGuidManager guidManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IArtifactGuidManager>())
        //    {
        //        return await guidManager.ReadSingleArtifactIdAsync(workspaceArtifactId, fieldGuid).ConfigureAwait(false);
        //    }
        //}

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

        private void GetJobHistoryDetails(int jobHistoryId, out int transferredItems, out int itemsWithError)
        {
            RelativityObject jobHistoryDetails = GetJobHistoryByObjectManager(jobHistoryId);

            transferredItems = (int)jobHistoryDetails.FieldValues.Where(f => f.Field.Name == "Items Transferred").FirstOrDefault().Value;
            itemsWithError = (int)jobHistoryDetails.FieldValues.Where(f => f.Field.Name == "Items with Errors").FirstOrDefault().Value;
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
