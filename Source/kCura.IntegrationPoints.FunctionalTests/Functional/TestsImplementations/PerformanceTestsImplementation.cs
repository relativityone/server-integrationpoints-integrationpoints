using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using NUnit.Framework;
using NUnit.Framework.Interfaces;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.API;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.ChoiceQuery;
using Relativity.Services.Folder;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Search;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;
using Choice = Relativity.Services.ChoiceQuery.Choice;
using KeywordSearch = Relativity.Testing.Framework.Models.KeywordSearch;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal class PerformanceTestsImplementation
    {
        private const string SavedSearchName = "AllDocuments";
        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;
        private Workspace[] _destinationWorkspaces;
        private List<int> _integrationPoints = new List<int>();
        
        private readonly IRipApi _ripApi =
            new RipApi(RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory);

        private int _savedSearchId;
        private int _destinationProviderId;
        private int _sourceProviderId;
        private int _integrationPointType;
        private List<Choice> _choices;

        public Workspace SourceWorkspace => _testsImplementationTestFixture.Workspace;

        public PerformanceTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
        {
            _testsImplementationTestFixture = testsImplementationTestFixture;
        }

        public void OnSetUpFixture(int runCount)
        {
            RelativityFacade.Instance.ImportDocumentsFromCsv(_testsImplementationTestFixture.Workspace,
                LoadFilesGenerator.GetOrCreateNativesLoadFile(), overwriteMode: DocumentOverwriteMode.AppendOverlay);
            
            CreateSavedSearch(_testsImplementationTestFixture.Workspace.ArtifactID);
            
            GetIntegrationPointsConstantsAsync().GetAwaiter().GetResult();

            _destinationWorkspaces = Enumerable.Range(0, runCount)
                .Select(i =>
                    RelativityFacade.Instance.CreateWorkspace(
                        string.Format(PerformanceTestsConstants.PERFORMANCE_TEST_WORKSPACE_NAME_FORMAT, i), SourceWorkspace.Name))
                .ToArray();
        }

        private void CreateSavedSearch(int workspaceId)
        {
            KeywordSearch keywordSearch = new KeywordSearch
            {
                Name = SavedSearchName
            };
            
            RelativityFacade.Instance.Resolve<IKeywordSearchService>()
                .Require(workspaceId, keywordSearch);
        }

        public void OnTearDownFixture()
        {
            foreach (var destinationWorkspace in _destinationWorkspaces)
            {
                RelativityFacade.Instance.DeleteWorkspace(destinationWorkspace);
            }

            if (TestContext.CurrentContext.Result.Outcome.Status == TestStatus.Passed)
            {
                DeleteIntegrationPoints(_integrationPoints, SourceWorkspace.ArtifactID);
            }
        }

        private void DeleteIntegrationPoints(IEnumerable<int> integrationPoints, int workspaceId)
        {
            MassDeleteByObjectIdentifiersRequest query = new MassDeleteByObjectIdentifiersRequest
            {
                Objects = new ReadOnlyCollection<RelativityObjectRef>(integrationPoints.Select(x => new RelativityObjectRef{ArtifactID = x}).ToList())
            };
            
            using (IObjectManager objectManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory
                .GetServiceProxy<IObjectManager>())
            {
                objectManager.DeleteAsync(workspaceId, query).GetAwaiter().GetResult();
            }
        }

        /// <summary>
        /// Returns measured time in seconds. Measures only time between job start and finish
        ///
        /// Job is started when status changes from Pending to anything else
        /// Job is ended when status is not Processing or Validating
        /// </summary>
        /// <returns>job time in seconds</returns>
        public async Task<double> RunSyncAndMeasureTime(int runIndex)
        {
            Workspace destinationWorkspace = _destinationWorkspaces[runIndex];

            IntegrationPointModel integrationPoint =
                await GetIntegrationPointAsync(destinationWorkspace).ConfigureAwait(false);
            await _ripApi.CreateIntegrationPointAsync(integrationPoint, SourceWorkspace.ArtifactID)
                .ConfigureAwait(false);
            
            _integrationPoints.Add(integrationPoint.ArtifactId);
            
            int jobHistoryId = await _ripApi.RunIntegrationPointAsync(integrationPoint, SourceWorkspace.ArtifactID)
                .ConfigureAwait(false);

            await WaitForJobToStartAsync(jobHistoryId, SourceWorkspace.ArtifactID, checkDelayInMs: 100).ConfigureAwait(false);
            Stopwatch stopwatch = Stopwatch.StartNew();

            await WaitForJobToFinishAsync(jobHistoryId, SourceWorkspace.ArtifactID, checkDelayInMs: 250);
            
            stopwatch.Stop();

            return stopwatch.Elapsed.Milliseconds / 1000d;
        }

        /// <summary>
        /// Runs <see cref="RunSyncAndMeasureTime"/> in a loop and calculates average time
        /// </summary>
        /// <param name="runCount"></param>
        /// <returns>Average execution time</returns>
        public async Task<double> RunPerformanceBenchmark(int runCount)
        {
            double runTimeSum = 0;
            for (int i = 0; i < runCount; i++)
            {
                runTimeSum += await RunSyncAndMeasureTime(i).ConfigureAwait(false);
            }

            return runTimeSum / runCount;
        }

        private Task WaitForJobToFinishAsync(int integrationPointId, int workspaceId, int checkDelayInMs = 500)
        {
            return WaitForJobStatus(integrationPointId, workspaceId, status =>
                status != PerformanceTestsConstants.JOB_STATUS_PROCESSING &&
                status != PerformanceTestsConstants.JOB_STATUS_VALIDATING, checkDelayInMs);
        }

        private Task WaitForJobToStartAsync(int jobHistoryId, int workspaceId, int checkDelayInMs = 500)
        {
            return WaitForJobStatus(jobHistoryId, workspaceId, status => status != PerformanceTestsConstants.JOB_STATUS_PENDING, checkDelayInMs);
        }

        private async Task WaitForJobStatus(int jobHistoryId, int workspaceId, Func<string, bool> waitUntil, int checkDelayInMs)
        {
            string status = await _ripApi.GetJobHistoryStatus(jobHistoryId, workspaceId);
            while (!waitUntil(status))
            {
                await Task.Delay(checkDelayInMs);
                status = await _ripApi.GetJobHistoryStatus(jobHistoryId, workspaceId).ConfigureAwait(false);
            }
        }

        private async Task<IntegrationPointModel> GetIntegrationPointAsync(Workspace destinationWorkspace)
        {
            List<FieldMap> fieldsMapping =
                await GetIdentifierMappingAsync(SourceWorkspace.ArtifactID, destinationWorkspace.ArtifactID)
                    .ConfigureAwait(false);
            
            int rootFolderId =
                await GetRootFolderArtifactIdAsync(destinationWorkspace.ArtifactID).ConfigureAwait(false);
            
            var sourceConfiguration = new RelativityProviderSourceConfiguration
            {
                TypeOfExport = (int)SourceConfiguration.ExportType.SavedSearch,
                SavedSearchArtifactId = _savedSearchId,
                SourceWorkspaceArtifactId = SourceWorkspace.ArtifactID,
                UseDynamicFolderPath = false
            };

            return new IntegrationPointModel
            {
                SourceConfiguration = sourceConfiguration,
                DestinationConfiguration =
                    GetDestinationConfiguration(destinationWorkspace.ArtifactID, rootFolderId),
                Name = string.Format(PerformanceTestsConstants.PERFORMANCE_TEST_INTEGRATION_POINT_NAME_FORMAT,
                    destinationWorkspace.ArtifactID),
                FieldMappings = fieldsMapping,
                DestinationProvider = _destinationProviderId,
                SourceProvider = _sourceProviderId,
                Type = _integrationPointType,
                EmailNotificationRecipients = "",
                OverwriteFieldsChoiceId = _choices.First(c => c.Name == "Append/Overlay").ArtifactID,
                ScheduleRule = new ScheduleModel()
            };
        }

        private async Task GetIntegrationPointsConstantsAsync()
        {
            _savedSearchId =
                await GetSavedSearchArtifactIdAsync(SourceWorkspace.ArtifactID, SavedSearchName).ConfigureAwait(false);

            _destinationProviderId =
                await GetDestinationProviderIdAsync(SourceWorkspace.ArtifactID).ConfigureAwait(false);

            _sourceProviderId = await GetSourceProviderIdAsync(SourceWorkspace.ArtifactID).ConfigureAwait(false);

            _integrationPointType = await GetIntegrationPointTypeAsync(SourceWorkspace.ArtifactID, "Export")
                .ConfigureAwait(false);

            _choices = await GetChoicesOnFieldAsync(SourceWorkspace.ArtifactID,
                Guid.Parse(IntegrationPointFieldGuids.OverwriteFields)).ConfigureAwait(false);
        }

        private async Task<int> GetIntegrationPointTypeAsync(int workspaceId, string typeName)
        {
            QueryRequest query = new QueryRequest()
            {
                ObjectType = new ObjectTypeRef
                {
                    Guid = ObjectTypeGuids.IntegrationPointTypeGuid
                },
                Condition = $"'Name' == '{typeName}'"
            };

            using (IObjectManager objectManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory
                .GetServiceProxy<IObjectManager>())
            {
                QueryResult result = await objectManager.QueryAsync(workspaceId, query, 0, 1).ConfigureAwait(false);
                return result.Objects.Single().ArtifactID;
            }
        }

        private ImportSettings GetDestinationSettings(int workspaceId, int folderId)
        {
            return new ImportSettings
            {
                ArtifactTypeId = (int) ArtifactType.Document,
                DestinationProviderType =
                    kCura.IntegrationPoints.Data.Constants.RELATIVITY_SOURCEPROVIDER_GUID.ToString(),
                CaseArtifactId = workspaceId,
                CreateSavedSearchForTagging = false,
                DestinationFolderArtifactId = folderId,
                Provider = "relativity",
                ImportOverwriteMode = ImportOverwriteModeEnum.AppendOnly,
                ImportNativeFile = false,
                ImportNativeFileCopyMode = ImportNativeFileCopyModeEnum.DoNotImportNativeFiles,
                UseDynamicFolderPath = false,
                ImageImport = false,
                ImagePrecedence = new ProductionDTO[0],
                ProductionPrecedence = "",
                IncludeOriginalImages = false,
                MoveExistingDocuments = false,
                ExtractedTextFieldContainsFilePath = false,
                ExtractedTextFileEncoding = "utf-16",
                EntityManagerFieldContainsLink = true,
                FieldOverlayBehavior = "Use Field Settings"
            };
        }

        private RelativityProviderDestinationConfiguration GetDestinationConfiguration(int workspaceId, int folderId)
        {
            return new RelativityProviderDestinationConfiguration
            {
                CaseArtifactId = workspaceId,
                FieldOverlayBehavior = RelativityProviderValidationMessages.FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_MERGE,
                ImportNativeFile = false,
                ArtifactTypeID = (int) ArtifactType.Document,
                DestinationFolderArtifactId = folderId,
                FolderPathSourceField = 0,
                UseFolderPathInformation = false
            };
        }

        private async Task<int> GetSavedSearchArtifactIdAsync(int workspaceId, string name = "All Documents")
        {
            using (IKeywordSearchManager keywordSearchManager = RelativityFacade.Instance.GetComponent<ApiComponent>()
                .ServiceFactory.GetServiceProxy<IKeywordSearchManager>())
            {
                Relativity.Services.Query request = new Relativity.Services.Query
                {
                    Condition = $"(('Name' == '{name}'))"
                };
                KeywordSearchQueryResultSet result =
                    await keywordSearchManager.QueryAsync(workspaceId, request).ConfigureAwait(false);
                if (result.TotalCount == 0)
                {
                    throw new InvalidOperationException(
                        $"Cannot find saved search '{name}' in workspace {workspaceId}");
                }

                return result.Results.First().Artifact.ArtifactID;
            }
        }

        private async Task<int> GetRootFolderArtifactIdAsync(int workspaceId)
        {
            using (IFolderManager folderManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory
                .GetServiceProxy<IFolderManager>())
            {
	            Relativity.Services.Folder.Folder rootFolder = await folderManager.GetWorkspaceRootAsync(workspaceId).ConfigureAwait(false);
                return rootFolder.ArtifactID;
            }
        }

        protected async Task<List<FieldMap>> GetIdentifierMappingAsync(int sourceWorkspaceId, int targetWorkspaceId)
        {
            using (IObjectManager objectManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory
                .GetServiceProxy<IObjectManager>())
            {
                QueryRequest query = PrepareIdentifierFieldsQueryRequest();
                QueryResult sourceQueryResult =
                    await objectManager.QueryAsync(sourceWorkspaceId, query, 0, 1).ConfigureAwait(false);
                QueryResult destinationQueryResult =
                    await objectManager.QueryAsync(targetWorkspaceId, query, 0, 1).ConfigureAwait(false);

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
            int fieldArtifactTypeID = (int) ArtifactType.Field;
            QueryRequest queryRequest = new QueryRequest()
            {
                ObjectType = new ObjectTypeRef()
                {
                    ArtifactTypeID = fieldArtifactTypeID
                },
                Condition = $"'FieldArtifactTypeID' == {(int) ArtifactType.Document} and 'Is Identifier' == true",
                Fields = new[] {new FieldRef {Name = "Name"}},
                IncludeNameInQueryResult = true
            };

            return queryRequest;
        }

        private async Task<int> GetSourceProviderIdAsync(int workspaceId,
            string identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.RELATIVITY)
        {
            QueryRequest query = new QueryRequest
            {
                Condition = $"'{SourceProviderFields.Identifier}' == '{identifier.ToLower()}'",
                ObjectType = new ObjectTypeRef {Guid = new Guid(ObjectTypeGuids.SourceProvider)},
                Fields = new FieldRef[] {new FieldRef {Name = "*"}}
            };

            using (IObjectManager objectManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory
                .GetServiceProxy<IObjectManager>())
            {
                QueryResult result = await objectManager.QueryAsync(workspaceId, query, 0, 1).ConfigureAwait(false);
                return result.Objects.Single().ArtifactID;
            }
        }

        private async Task<int> GetDestinationProviderIdAsync(int workspaceId,
            string identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.DestinationProviders
                .RELATIVITY)
        {
            QueryRequest query = new QueryRequest
            {
                Condition = $"'{DestinationProviderFields.Identifier}' == '{identifier}'",
                ObjectType = new ObjectTypeRef {Guid = new Guid(ObjectTypeGuids.DestinationProvider)}
            };

            using (var objectManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory
                .GetServiceProxy<IObjectManager>())
            {
                QueryResult result = await objectManager.QueryAsync(workspaceId, query, 0, 10000).ConfigureAwait(false);

                return result.Objects.Single().ArtifactID;
            }
        }
        
        private async Task<List<Choice>> GetChoicesOnFieldAsync(int workspaceArtifactId, Guid fieldGuid)
        {
            int fieldId = await ReadFieldIdByGuidAsync(workspaceArtifactId, fieldGuid).ConfigureAwait(false);

            using (IChoiceQueryManager choiceManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IChoiceQueryManager>())
            {
                return await choiceManager.QueryAsync(workspaceArtifactId, fieldId).ConfigureAwait(false);
            }
        }

        private async Task<int> ReadFieldIdByGuidAsync(int workspaceArtifactId, Guid fieldGuid)
        {
            using (IArtifactGuidManager guidManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IArtifactGuidManager>())
            {
                return await guidManager.ReadSingleArtifactIdAsync(workspaceArtifactId, fieldGuid).ConfigureAwait(false);
            }
        }
    }
}