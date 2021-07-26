using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Contracts.Configuration;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Newtonsoft.Json;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.API;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using Relativity.Services.Folder;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Search;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Models;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal class PerformanceTestsImplementation
    {
        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;
        private Workspace[] _destinationWorkspaces;

        private readonly IRipApi _ripApi =
            new RipApi(RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory);

        public Workspace SourceWorkspace => _testsImplementationTestFixture.Workspace;

        public PerformanceTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
        {
            _testsImplementationTestFixture = testsImplementationTestFixture;
        }

        public void OnSetUpFixture()
        {
            RelativityFacade.Instance.ImportDocumentsFromCsv(_testsImplementationTestFixture.Workspace,
                LoadFilesGenerator.GetOrCreateNativesLoadFile(), overwriteMode: DocumentOverwriteMode.AppendOverlay);

            RelativityFacade.Instance.RequireAgent(Const.INTEGRATION_POINTS_AGENT_TYPE_NAME,
                Const.INTEGRATION_POINTS_AGENT_RUN_INTERVAL);

            _destinationWorkspaces = Enumerable.Range(0, PerformanceTestsConstants.RUN_COUNT)
                .Select(i =>
                    RelativityFacade.Instance.CreateWorkspace(
                        string.Format(PerformanceTestsConstants.PERFORMANCE_TEST_WORKSPACE_NAME_FORMAT, i)))
                .ToArray();
        }

        public void OnTearDownFixture()
        {
            foreach (var destinationWorkspace in _destinationWorkspaces)
            {
                RelativityFacade.Instance.DeleteWorkspace(destinationWorkspace);
            }

            // RelativityFacade.Instance.DeleteWorkspace(SourceWorkspace);
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
            // await _ripApi.CreateIntegrationPointAsync(integrationPoint, SourceWorkspace.ArtifactID)
            //     .ConfigureAwait(false);
            //
            // await _ripApi.RunIntegrationPointAsync(integrationPoint, SourceWorkspace.ArtifactID)
            //     .ConfigureAwait(false);

            await WaitForJobToStartAsync(1040323, SourceWorkspace.ArtifactID).ConfigureAwait(false);
            DateTime startTime = DateTime.Now;

            await WaitForJobToFinishAsync(1040323, SourceWorkspace.ArtifactID);
            DateTime endTime = DateTime.Now;

            return (endTime - startTime).TotalSeconds;
        }

        /// <summary>
        /// Runs <see cref="RunSyncAndMeasureTime"/> in a loop and calculates average time
        /// </summary>
        /// <returns>Average execution time</returns>
        public async Task<double> RunPerformanceBenchmark()
        {
            double runTimeSum = 0;
            for (int i = 0; i < PerformanceTestsConstants.RUN_COUNT; i++)
            {
                runTimeSum += await RunSyncAndMeasureTime(i).ConfigureAwait(false);
            }

            return runTimeSum / PerformanceTestsConstants.RUN_COUNT;
        }

        private Task WaitForJobToFinishAsync(int jobId, int workspaceId)
        {
            return WaitForJobStatus(jobId,
                status =>
                    status != PerformanceTestsConstants.JOB_STATUS_PROCESSING &&
                    status != PerformanceTestsConstants.JOB_STATUS_VALIDATING, workspaceId);
        }

        private Task WaitForJobToStartAsync(int jobId, int workspaceId)
        {
            return WaitForJobStatus(jobId, status => status != PerformanceTestsConstants.JOB_STATUS_PENDING, workspaceId);
        }

        private async Task WaitForJobStatus(int jobId, Func<string, bool> waitUntil, int workspaceId)
        {
            string status = await _ripApi.GetJobHistoryStatus(jobId, workspaceId);
            while (!waitUntil(status))
            {
                await Task.Delay(500);
                status = await _ripApi.GetJobHistoryStatus(jobId, workspaceId).ConfigureAwait(false);
            }
        }

        private async Task<IntegrationPointModel> GetIntegrationPointAsync(Workspace destinationWorkspace)
        {
            int rootFolderId =
                await GetRootFolderArtifactIdAsync(destinationWorkspace.ArtifactID).ConfigureAwait(false);
            int savedSearchId =
                await GetSavedSearchArtifactIdAsync(SourceWorkspace.ArtifactID).ConfigureAwait(false);
            List<FieldMap> fieldsMapping =
                await GetIdentifierMappingAsync(SourceWorkspace.ArtifactID, destinationWorkspace.ArtifactID)
                    .ConfigureAwait(false);

            int destinationProviderId =
                await GetDestinationProviderIdAsync(SourceWorkspace.ArtifactID).ConfigureAwait(false);
            int sourceProviderId = await GetSourceProviderIdAsync(SourceWorkspace.ArtifactID).ConfigureAwait(false);
            int integrationPointType = await GetIntegrationPointTypeAsync(SourceWorkspace.ArtifactID, "Export")
                .ConfigureAwait(false);

            return new IntegrationPointModel
            {
                SourceConfiguration = new SourceConfiguration
                {
                    SavedSearch = savedSearchId.ToString(),
                    TypeOfExport = SourceConfiguration.ExportType.SavedSearch,
                    SourceWorkspaceArtifactId = SourceWorkspace.ArtifactID,
                    TargetWorkspace = destinationWorkspace.ArtifactID.ToString(),
                },
                DestinationConfiguration =
                    JsonConvert.SerializeObject(GetDestinationSettings(destinationWorkspace.ArtifactID, rootFolderId)),
                Name = string.Format(PerformanceTestsConstants.PERFORMANCE_TEST_INTEGRATION_POINT_NAME_FORMAT,
                    destinationWorkspace.ArtifactID),
                FieldMappings = fieldsMapping,
                DestinationProvider = destinationProviderId,
                SourceProvider = sourceProviderId,
                Type = integrationPointType,
                EmailNotificationRecipients = "",
            };
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
                Folder rootFolder = await folderManager.GetWorkspaceRootAsync(workspaceId).ConfigureAwait(false);
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
    }
}