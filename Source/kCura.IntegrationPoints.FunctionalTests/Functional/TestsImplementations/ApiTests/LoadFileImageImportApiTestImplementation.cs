using FluentAssertions;
using kCura.IntegrationPoints.Synchronizers.RDO;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Functional.DataModels;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.API;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static kCura.IntegrationPoints.Core.Constants.IntegrationPoints;
using static Relativity.IntegrationPoints.Tests.Functional.Const;
using Document = Relativity.Testing.Framework.Models.Document;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal class LoadFileImageImportApiTestImplementation
    {
        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;

        private readonly IRipApi _ripApi = 
            new RipApi(RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory);

        public Workspace SourceWorkspace => _testsImplementationTestFixture.Workspace;

        public LoadFileImageImportApiTestImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
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
            int expectedNumberOfDocs = 10;
            IntegrationPointModel integrationPoint = await GetIntegrationPointAsync();
            await _ripApi.CreateIntegrationPointAsync(integrationPoint, SourceWorkspace.ArtifactID).ConfigureAwait(false);

            // Act
            int jobHistoryId = await _ripApi.RunIntegrationPointAsync(integrationPoint, SourceWorkspace.ArtifactID).ConfigureAwait(false);
            await _ripApi.WaitForJobToFinishAsync(jobHistoryId, SourceWorkspace.ArtifactID);
            List<RelativityObject> workspaceDocs = GetAllDocumentsFromWorkspace(SourceWorkspace.ArtifactID);

            //Assert

            Document[] docs = RelativityFacade.Instance.Resolve<IDocumentService>().GetAll(SourceWorkspace.ArtifactID);

            docs.Should().HaveCount(expectedNumberOfDocs);

            docs.Should().OnlyContain(x => x.HasImages.Name == "Yes");
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
            ICommonIntegrationPointDataService commonDataSvc = new CommonIntegrationPointDataService(
                RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory, SourceWorkspace.ArtifactID);

            int destinationFodler = await commonDataSvc.GetRootFolderArtifactIdAsync().ConfigureAwait(false);

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
                DestinationFolderArtifactId = destinationFodler.ToString(),
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
                DestinationFolderArtifactId = destinationFodler.ToString()
            };

            return new IntegrationPointModel
            {
                SourceConfiguration = sourceConfiguration,
                DestinationConfiguration = destinationConfiguration,
                Name = nameof(LoadFileImageImportApiTestImplementation),
                DestinationProvider = await commonDataSvc.GetDestinationProviderIdAsync(DestinationProviders.RELATIVITY).ConfigureAwait(false),
                SourceProvider = await commonDataSvc.GetSourceProviderIdAsync(SourceProviders.IMPORTLOADFILE).ConfigureAwait(false),
                Type = await commonDataSvc.GetIntegrationPointTypeByAsync(IntegrationPointTypes.ImportName).ConfigureAwait(false),
                EmailNotificationRecipients = "",
                FieldMappings = new List<FieldMap>(),
                OverwriteFieldsChoiceId = await commonDataSvc.GetOverwriteFieldsChoiceIdAsync(ImportOverwriteModeEnum.AppendOnly).ConfigureAwait(false),
                ScheduleRule = new ScheduleModel(),
                LogErrors = true
            };
        }
    }
}
