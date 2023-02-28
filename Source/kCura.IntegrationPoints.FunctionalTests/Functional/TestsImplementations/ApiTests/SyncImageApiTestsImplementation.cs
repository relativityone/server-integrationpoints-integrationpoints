using System.Threading.Tasks;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator;
using Relativity.IntegrationPoints.Tests.Functional.DataModels;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Models;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations.ApiTests
{
    internal class SyncImageApiTestsImplementation : SyncApiTestsImplementationBase
    {
        private readonly ImageImportOptions _imageImportOptions = new ImageImportOptions
        {
            ExtractedTextFieldContainsFilePath = false,
            OverwriteMode = DocumentOverwriteMode.AppendOverlay,
            OverlayBehavior = DocumentOverlayBehavior.UseRelativityDefaults,
            FileLocationField = "FileLocation",
            IdentityFieldId = 0,
            DocumentIdentifierField = "DocumentIdentifier",
            ExtractedTextEncoding = null,
            BatesNumberField = "BatesNumber"
        };

        public SyncImageApiTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture) :
            base(testsImplementationTestFixture)
        {
        }

        public void OneTimeSetup()
        {
            void ImportAction()
            {
                const int imagesCount = 10;

                string testDataPath = LoadFilesGenerator.GetOrCreateNativesOptLoadFile();
                RelativityFacade.Instance.ImportImages(TestsImplementationTestFixture.Workspace, testDataPath
                    , _imageImportOptions, imagesCount);
            }

            OneTimeSetupExecution(ImportAction);
        }

        public async Task RunAndRetryIntegrationPoint()
        {
            void ImportAction(Workspace destinationWorkspace)
            {
                const int destinationWorkspaceInitialImportCount = 4;
                string testDataPath = LoadFilesGenerator.GetOrCreateNativesOptLoadFileWithLimitedItems(destinationWorkspaceInitialImportCount);
                RelativityFacade.Instance.ImportImages(TestsImplementationTestFixture.Workspace, testDataPath
                    , _imageImportOptions, destinationWorkspaceInitialImportCount);
            }

            await RunAndRetryIntegrationPointExecution(ImportAction);
        }

        protected override DestinationConfiguration GetDestinationConfiguration(int destinationWorkspaceId,
            int destinationFolderId)
        {
            DestinationConfiguration destinationConfiguration = new DestinationConfiguration
            {
                ImageImport = true,
                CaseArtifactId = destinationWorkspaceId,
                FieldOverlayBehavior = RelativityProviderValidationMessages.FIELD_MAP_FIELD_OVERLAY_BEHAVIOR_DEFAULT,
                ImportNativeFile = false,
                ArtifactTypeID = (int)ArtifactType.Document,
                DestinationFolderArtifactId = destinationFolderId,
                FolderPathSourceField = 0,
                UseFolderPathInformation = false
            };
            return destinationConfiguration;
        }
    }
}
