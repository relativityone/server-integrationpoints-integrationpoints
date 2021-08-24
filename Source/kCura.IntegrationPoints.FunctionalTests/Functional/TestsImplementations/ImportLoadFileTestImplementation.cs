using Atata;
using System.IO;
using NUnit.Framework;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Web.Navigation;
using Relativity.Testing.Framework.Web.Extensions;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using System.Threading.Tasks;
using FluentAssertions;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal class ImportLoadFileTestImplementation
    {
        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;

        public ImportLoadFileTestImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
        {
            _testsImplementationTestFixture = testsImplementationTestFixture;
        }

        public async Task OnSetUpFixture()
        {
            // Installing necessary app
            if (RelativityFacade.Instance.Resolve<ILibraryApplicationService>().Get("ARM Test Services") != null)
            {
                SetDevelopmentModeToTrue();
                InstallARMTestServicesToWorkspace();
            }
            
            // Preparing data for LoadFile and placing it in the right location
            LoadFilesGenerator.GetOrCreateNativesDatLoadFile();
            string destinationPath = await LoadFilesGenerator.GetFilesharePathAsync(_testsImplementationTestFixture.Workspace.ArtifactID);
            destinationPath = Path.Combine(destinationPath, "DataTransfer\\Import");

            await LoadFilesGenerator.UploadDirectoryAsync(destinationPath).ConfigureAwait(false);
        }

        private void SetDevelopmentModeToTrue()
        {
            IInstanceSettingsService instanceSettingsService = RelativityFacade.Instance.Resolve<IInstanceSettingsService>();

            var developmentModeSetting = instanceSettingsService.Get("DevelopmentMode", "kCura.ARM");
            if (developmentModeSetting == null)
            {
                instanceSettingsService.Create(new Testing.Framework.Models.InstanceSetting
                {
                    Name = "DevelopmentMode",
                    Section = "kCura.ARM",
                    Value = "True",
                    ValueType = InstanceSettingValueType.TrueFalse
                });
            }
        }

        private static void InstallARMTestServicesToWorkspace()
        {
            string rapFileLocation = Path.Combine(TestContext.Parameters["BuildToolsDirectory"], "ARMTestServices.RAP\\lib\\ARMTestServices.rap");

            RelativityFacade.Instance.Resolve<ILibraryApplicationService>()
                .InstallToLibrary(rapFileLocation, new LibraryApplicationInstallOptions
            {
                CreateIfMissing = true
            });
        }

        public void ImportNativesFromLoadFileGoldFlow()
        {
            // Arrange
            _testsImplementationTestFixture.LoginAsStandardUser();

            string integrationPointName = nameof(ImportNativesFromLoadFileGoldFlow);

            // Act 
            var integrationPointListPage = Being.On<IntegrationPointListPage>(_testsImplementationTestFixture.Workspace.ArtifactID);
            var integrationPointEditPage = integrationPointListPage.NewIntegrationPoint.ClickAndGo();

            var importFromLoadFileConnectToSourcePage = FillOutIntegrationPointEditPageForImportFromLoadFile(integrationPointEditPage, integrationPointName);
            importFromLoadFileConnectToSourcePage.WorkspaceDestinationFolder.Click().SetItem($"{_testsImplementationTestFixture.Workspace.Name}");
            importFromLoadFileConnectToSourcePage.ImportSource.Click().SetItem("NativesLoadFile.dat");
            importFromLoadFileConnectToSourcePage.Column.Set("| (ASCII:124)");
            importFromLoadFileConnectToSourcePage.Quote.Set("^ (ASCII:094)");
            var importFromLoadFileMapFieldsPage = importFromLoadFileConnectToSourcePage.Next.ClickAndGo();

            importFromLoadFileMapFieldsPage.MapAllFields.Click();
            importFromLoadFileMapFieldsPage.ApplyModel(new ImportLoadFileMapFields());
            importFromLoadFileMapFieldsPage.NativeFilePath.Set("FILE_PATH");
            importFromLoadFileMapFieldsPage.FolderPathInformation.Set("Folder_Path");

            var integrationPointViewPage = importFromLoadFileMapFieldsPage.Save.ClickAndGo();

            integrationPointViewPage = IntegrationPointViewPage.RunIntegrationPoint(integrationPointViewPage, integrationPointName);

            // Assert
            int transferredItemsCount = IntegrationPointViewPage.GetTransferredItemsCount(integrationPointViewPage, integrationPointName);
            int workspaceEntityCount = RelativityFacade.Instance.Resolve<IDocumentService>().GetAll(_testsImplementationTestFixture.Workspace.ArtifactID).Length;
            transferredItemsCount.Should().Be(workspaceEntityCount).And.Be(9);
        }

        private static ImportFromLoadFileConnectToSourcePage FillOutIntegrationPointEditPageForImportFromLoadFile(IntegrationPointEditPage integrationPointEditPage, string integrationPointName)
        {
            IntegrationPointEdit integrationPointEdit = new IntegrationPointEditImport
            {
                Source = IntegrationPointSources.LoadFile,
                TransferredObject = IntegrationPointTransferredObjects.Document,
                Name = integrationPointName
            };

            return integrationPointEditPage.ApplyModel(integrationPointEdit).ImportFromLoadFileNext.ClickAndGo();
        }
    }
}
