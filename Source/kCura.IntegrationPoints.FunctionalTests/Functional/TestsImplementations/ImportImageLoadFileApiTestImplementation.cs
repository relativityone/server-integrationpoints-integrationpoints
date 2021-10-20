using FluentAssertions;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Functional.DataModels;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.API;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using Relativity.Services.ArtifactGuid;
using Relativity.Services.ChoiceQuery;
using Relativity.Services.Folder;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static Relativity.IntegrationPoints.Tests.Functional.Const;
using Choice = Relativity.Services.ChoiceQuery.Choice;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal class ImportImageLoadFileApiTestImplementation
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

        public ImportImageLoadFileApiTestImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
        {
            _testsImplementationTestFixture = testsImplementationTestFixture;
        }

        public void OnSetUpFixture()
        {
            string testDataPath = LoadFilesGenerator.GetOrCreateNativesOptLoadFile();
            LoadFilesGenerator.UploadLoadFileToImportDirectory(_testsImplementationTestFixture.Workspace.ArtifactID, testDataPath).Wait();
        }

        public async Task RunIntegrationPointAndCheckCorectness()
        {
            // Arrange
            int numberOfDocs = 10;
            string hasImagesFieldName = "Has Images";
            string hasImagesFieldValue = "Yes";
            IntegrationPointModel integrationPoint = await GetIntegrationPointAsync();
            await _ripApi.CreateIntegrationPointAsync(integrationPoint, SourceWorkspace.ArtifactID).ConfigureAwait(false);

            // Act
            int jobHistoryId = await _ripApi.RunIntegrationPointAsync(integrationPoint, SourceWorkspace.ArtifactID).ConfigureAwait(false);
            await _ripApi.WaitForJobToFinishSuccessfullyAsync(jobHistoryId, SourceWorkspace.ArtifactID, checkDelayInMs: 250);
            List<RelativityObject> workspaceDocs = GetAllDocumentsFromWorkspace(SourceWorkspace.ArtifactID);

            //Assert
            workspaceDocs.Count.Should().Be(numberOfDocs);
            foreach (var doc in workspaceDocs)
            {
                var hasImages = doc.FieldValues.Find(x => x.Field.Name == hasImagesFieldName);
                hasImages.Value.ShouldBeEquivalentTo(new { Value = hasImagesFieldValue }, options => options.ExcludingMissingMembers());
            }
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

        public async Task<IntegrationPointModel> GetIntegrationPointAsync()
        {
            _workspaceDestinationFolder = await GetRootFolderArtifactIdAsync(SourceWorkspace.ArtifactID);

            var sourceConfiguration = new ImportSourceConfigurationModel()
            {
                HasColumnName = "true",
                EncodingType = "utf-8",
                AsciiColumn = ImportLoadFile.ASCII_COLUMN,
                AsciiQuote = ImportLoadFile.ASCII_QUOTE,
                AsciiNewLine = ImportLoadFile.ASCII_NEWLINE,
                AsciiMultiLine = ImportLoadFile.ASCII_MULTILINE,
                AsciiNestedValue = ImportLoadFile.ASCII_NESTEDVALUE,
                WorkspaceId = SourceWorkspace.ArtifactID.ToString(),
                ImportType = (int)ImportSourceConfigurationModel.ImportTypeEnum.ImageLoadFile,
                LoadFile = "DataTransfer\\Import\\ImagesLoadFile.opt",
                LineNumber = "0",
                DestinationFolderArtifactId = _workspaceDestinationFolder.ToString(),
                ImageImport = true,
                ForProduction = false,
                AutoNumberImages = "false",
                ImportOverwriteMode = ImportOverwriteModeEnum.AppendOnly.ToString(),
                ExtractedTextFieldContainsFilePath = "false",
                ExtractedTextFileEncoding = "UTF-8",
                CopyFilesToDocumentRepository = "true",
                SelectedCaseFileRepoPath = "\\\\emttest\\DefaultFileRepository\\",
                ImportNativeFileCopyMode = ImportNativeFileCopyModeEnum.CopyFiles.ToString()
            };

            var destinationConfiguration = new ImportDestinationConfigurationModel()
            {
                artifactTypeID = (int)ArtifactType.Document,
                CaseArtifactId = SourceWorkspace.ArtifactID,
                ImageImport = true,
                ForProduction = false,
                AutoNumberImages = "false",
                ImportOverwriteMode = ImportOverwriteModeEnum.AppendOnly.ToString(),
                ExtractedTextFieldContainsFilePath = "false",
                ExtractedTextFileEncoding = "UTF-8",
                CopyFilesToDocumentRepository = "true",
                SelectedCaseFileRepoPath = "\\\\emttest\\DefaultFileRepository\\",
                ImportNativeFileCopyMode = ImportNativeFileCopyModeEnum.CopyFiles.ToString(),
                WorkspaceId = SourceWorkspace.ArtifactID.ToString(),
                ImportType = (int) ImportDestinationConfigurationModel.ImportTypeEnum.ImageLoadFile,
                LoadFile = "DataTransfer\\Import\\ImagesLoadFile.opt",
                LineNumber = "0",
                DestinationFolderArtifactId = _workspaceDestinationFolder.ToString()
            };

            _sourceProviderId = await GetSourceProviderAsync(SourceWorkspace.ArtifactID);
            _destinationProviderId = await GetDestinationProviderIdAsync(SourceWorkspace.ArtifactID);
            _integrationPointTypeId = await GetIntegrationPointTypeAsync(SourceWorkspace.ArtifactID, "Import");
            _choices = await GetChoicesOnFieldAsync(SourceWorkspace.ArtifactID, Guid.Parse(IntegrationPointFieldGuids.OverwriteFields)).ConfigureAwait(false);
            string name = "TEST_FOR_LOADFILE_IMPORT_IMAGES";
            return new IntegrationPointModel
            {
                SourceConfiguration = sourceConfiguration,
                DestinationConfiguration = destinationConfiguration,
                Name = name,
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
