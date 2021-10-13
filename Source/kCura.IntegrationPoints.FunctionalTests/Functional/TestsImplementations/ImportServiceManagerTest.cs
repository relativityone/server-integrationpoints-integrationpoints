using FluentAssertions;
using kCura.IntegrationPoints.Data;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Functional.DataModels;
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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Choice = Relativity.Services.ChoiceQuery.Choice;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal class ImportServiceManagerTest
    {
        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;

        private readonly IRipApi _ripApi = 
            new RipApi(RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory);

        private int _integrationPointTypeId;
        private int _workspaceDestinationFolder;
        private int _sourceProviderId;
        private int _destinationProviderId;
        private List<Choice> _choices;

        public Workspace SourceWorkspace => _testsImplementationTestFixture.Workspace;

        public ImportServiceManagerTest(ITestsImplementationTestFixture testsImplementationTestFixture)
        {
            _testsImplementationTestFixture = testsImplementationTestFixture;
        }

        public void OnSetUpFixture()
        {
            if (RelativityFacade.Instance.Resolve<ILibraryApplicationService>().Get("ARM Test Services") == null)
            {
                InstallARMTestServicesToWorkspace();
            }

            // Preparing data for LoadFile and placing it in the right location
            string testDataPath = LoadFilesGenerator.GetOrCreateNativesOptLoadFile();
            LoadFilesGenerator.UploadLoadFileToImportDirectory(_testsImplementationTestFixture.Workspace.ArtifactID, testDataPath).Wait();

            GetIntegrationPointsConstantsAsync().GetAwaiter().GetResult();
        }

        public async Task RunIntegrationPointAndCheckCorectness()
        {
            IntegrationPointModel integrationPoint = GetIntegrationPointAsync();
            await _ripApi.CreateIntegrationPointAsync(integrationPoint, SourceWorkspace.ArtifactID).ConfigureAwait(false);

            int jobHistoryId = await _ripApi.RunIntegrationPointAsync(integrationPoint, SourceWorkspace.ArtifactID).ConfigureAwait(false);
            await WaitForJobToFinishSuccessfullyAsync(jobHistoryId, SourceWorkspace.ArtifactID, checkDelayInMs: 250);
            List<RelativityObject> workspaceDocs = GetAllDocumentsFromWorkspace(SourceWorkspace.ArtifactID);
            workspaceDocs.Count.Should().Be(10);
        }

        private static void InstallARMTestServicesToWorkspace()
        {
            string rapFileLocation = TestConfig.ARMTestServicesRapFileLocation;

            RelativityFacade.Instance.Resolve<ILibraryApplicationService>()
                .InstallToLibrary(rapFileLocation, new LibraryApplicationInstallOptions
                {
                    CreateIfMissing = true
                });
        }

        private List<RelativityObject> GetAllDocumentsFromWorkspace(int workspaceId)
        {
            using (IObjectManager objectManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IObjectManager>())
            {
                QueryRequest request = new QueryRequest
                {
                    ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Document },
                    Fields = new FieldRef[] { new FieldRef { Name = "*" } }
                };

                return objectManager.QueryAsync(workspaceId, request, 0, int.MaxValue)
                    .GetAwaiter().GetResult().Objects.ToList();
            }
        }

        private Task WaitForJobToFinishSuccessfullyAsync(int integrationPointId, int workspaceId, int checkDelayInMs = 500)
        {
            return WaitForJobStatus(integrationPointId, workspaceId, status =>
                status == JobStatusChoices.JobHistoryCompleted.Name, checkDelayInMs);
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

        public IntegrationPointModel GetIntegrationPointAsync()
        {
            var sourceConfiguration = new ImportSourceConfigurationModel()
            {
                HasColumnName = "true",
                EncodingType = "utf-8",
                AsciiColumn = 20,
                AsciiQuote = 254,
                AsciiNewLine = 174,
                AsciiMultiLine = 59,
                AsciiNestedValue = 92,
                WorkspaceId = SourceWorkspace.ArtifactID.ToString(),
                ImportType = (int)ImportSourceConfigurationModel.ImportTypeEnum.ImageLoadFile,
                LoadFile = "DataTransfer\\Import\\ImagesLoadFile.opt",
                LineNumber = "0",
                DestinationFolderArtifactId = _workspaceDestinationFolder.ToString(),
                ImageImport = true,
                ForProduction = false,
                AutoNumberImages = "false",
                ImportOverwriteMode = "AppendOnly",
                //IdentityFieldId = 1003667, //no idea what is this
                ExtractedTextFieldContainsFilePath = "false",
                ExtractedTextFileEncoding = "UTF-8",
                CopyFilesToDocumentRepository = "true",
                SelectedCaseFileRepoPath = "\\\\emttest\\DefaultFileRepository\\",
                ImportNativeFileCopyMode = "CopyFiles"
            };

            var destinationConfiguration = new ImportDestinationConfigurationModel()
            {
                artifactTypeID = (int)ArtifactType.Document,
                //destinationProviderType = "74A863B9-00EC-4BB7-9B3E-1E22323010C6",
                CaseArtifactId = SourceWorkspace.ArtifactID,
                ImageImport = true,
                ForProduction = false,
                AutoNumberImages = "false",
                ImportOverwriteMode = "AppendOnly",
                //IdentityFieldId = 1003667, //no idea what is this
                ExtractedTextFieldContainsFilePath = "false",
                ExtractedTextFileEncoding = "UTF-8",
                CopyFilesToDocumentRepository = "true",
                SelectedCaseFileRepoPath = "\\\\emttest\\DefaultFileRepository\\",
                ImportNativeFileCopyMode = "CopyFiles",
                WorkspaceId = SourceWorkspace.ArtifactID.ToString(),
                ImportType = (int) ImportDestinationConfigurationModel.ImportTypeEnum.ImageLoadFile,
                LoadFile = "DataTransfer\\Import\\ImagesLoadFile.opt",
                LineNumber = "0",
                DestinationFolderArtifactId = _workspaceDestinationFolder.ToString()
            };

            return new IntegrationPointModel
            {
                SourceConfiguration = sourceConfiguration,
                DestinationConfiguration = destinationConfiguration,
                Name = Const.INTEGRATION_POINT_NAME_FOR_LOADFILE_IMPORT_IMAGES,
                DestinationProvider = _destinationProviderId,
                SourceProvider = _sourceProviderId,
                Type = _integrationPointTypeId,
                EmailNotificationRecipients = "",
                FieldMappings = new List<FieldMap>(),
                OverwriteFieldsChoiceId = _choices.First(c => c.Name == "Append Only").ArtifactID,
                ScheduleRule = new ScheduleModel(),
                LogErrors = true
            };
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

        private async Task<int> ReadFieldIdByGuidAsync(int workspaceArtifactId, Guid fieldGuid)
        {
            using (IArtifactGuidManager guidManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IArtifactGuidManager>())
            {
                return await guidManager.ReadSingleArtifactIdAsync(workspaceArtifactId, fieldGuid).ConfigureAwait(false);
            }
        }

        private async Task GetIntegrationPointsConstantsAsync()
        {
            _sourceProviderId = await GetSourceProviderAsync(SourceWorkspace.ArtifactID);
            _destinationProviderId = await GetDestinationProviderIdAsync(SourceWorkspace.ArtifactID);
            _integrationPointTypeId = await GetIntegrationPointTypeAsync(SourceWorkspace.ArtifactID, "Import");
            _workspaceDestinationFolder = await GetRootFolderArtifactIdAsync(SourceWorkspace.ArtifactID);
            _choices = await GetChoicesOnFieldAsync(SourceWorkspace.ArtifactID, Guid.Parse(IntegrationPointFieldGuids.OverwriteFields)).ConfigureAwait(false);
        }

        private async Task<int> GetFieldArtifactIdByItsNameAsync(int workspaceId, string name)
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

        private async Task<List<Choice>> GetChoicesOnFieldAsync(int workspaceArtifactId, Guid fieldGuid)
        {
            int fieldId = await ReadFieldIdByGuidAsync(workspaceArtifactId, fieldGuid).ConfigureAwait(false);

            using (IChoiceQueryManager choiceManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IChoiceQueryManager>())
            {
                return await choiceManager.QueryAsync(workspaceArtifactId, fieldId).ConfigureAwait(false);
            }
        }

        private async Task<int> GetSourceProviderAsync(int workspaceId)
        {
            string identifier = kCura.IntegrationPoints.Core.Constants.IntegrationPoints.SourceProviders.IMPORTLOADFILE;

            QueryRequest query = new QueryRequest
            {
                Condition = $"'{SourceProviderFields.Identifier}' == '{identifier.ToLower()}'",
                ObjectType = new ObjectTypeRef { Guid = new Guid(ObjectTypeGuids.SourceProvider) },
                Fields = new FieldRef[] { new FieldRef { Name = "*" } }
            };

            using (IObjectManager objectManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IObjectManager>())
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
                ObjectType = new ObjectTypeRef { Guid = new Guid(ObjectTypeGuids.DestinationProvider) }
            };

            using (var objectManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory
                .GetServiceProxy<IObjectManager>())
            {
                QueryResult result = await objectManager.QueryAsync(workspaceId, query, 0, 10000).ConfigureAwait(false);

                return result.Objects.Single().ArtifactID;
            }
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

            using(IObjectManager objectManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IObjectManager>())
            {
                QueryResult result = await objectManager.QueryAsync(workspaceId, query, 0, 1).ConfigureAwait(false);
                return result.Objects.Single().ArtifactID;
            }
        }

    }
}
