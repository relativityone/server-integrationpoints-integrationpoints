using Atata;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Web.Navigation;
using Relativity.Testing.Framework.Web.Extensions;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using FluentAssertions;
using Relativity.IntegrationPoints.Tests.Functional.Web.Extensions;
using Relativity.Testing.Framework.Web.Models;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal class ImportLoadFileTestImplementation
    {
        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;

        public ImportLoadFileTestImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
        {
            _testsImplementationTestFixture = testsImplementationTestFixture;
        }

        public void OnSetUpFixture()
        {            
            // Preparing data for LoadFile and placing it in the right location
            string testDataPath = LoadFilesGenerator.GetOrCreateNativesDatLoadFile();
            LoadFilesGenerator.UploadLoadFileToImportDirectory(_testsImplementationTestFixture.Workspace.ArtifactID, testDataPath).Wait();
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
            var importFromLoadFileMapFieldsPage = FillOutIntegrationPointConnectToSourcePageForImportFromLoadFile(importFromLoadFileConnectToSourcePage, _testsImplementationTestFixture.Workspace.Name);

            importFromLoadFileMapFieldsPage.MapAllFields.Click();
            importFromLoadFileMapFieldsPage.ApplyModel(new ImportLoadFileMapFields
            {
                CopyNativeFiles = RelativityProviderCopyNativeFiles.PhysicalFiles,
                UseFolderPathInformation = YesNo.Yes
            });
            importFromLoadFileMapFieldsPage.NativeFilePath.Set("FILE_PATH");
            importFromLoadFileMapFieldsPage.FolderPathInformation.Set("Folder_Path");

            var integrationPointViewPage = importFromLoadFileMapFieldsPage.Save.ClickAndGo();

            integrationPointViewPage = integrationPointViewPage.RunIntegrationPoint(integrationPointName);

            // Assert
            int transferredItemsCount = integrationPointViewPage.GetTransferredItemsCount(integrationPointName);
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

        private static ImportFromLoadFileMapFieldsPage FillOutIntegrationPointConnectToSourcePageForImportFromLoadFile(ImportFromLoadFileConnectToSourcePage importFromLoadFileConnectToSourcePage, string folderName)
        {
            importFromLoadFileConnectToSourcePage.WorkspaceDestinationFolder.Click().SetTreeItem($"{folderName}");
            importFromLoadFileConnectToSourcePage.ImportSource.Click().SetTreeItem("NativesLoadFile.dat");
            importFromLoadFileConnectToSourcePage.Column.Set("| (ASCII:124)");
            importFromLoadFileConnectToSourcePage.Quote.Set("^ (ASCII:094)");
            return importFromLoadFileConnectToSourcePage.Next.ClickAndGo();
        }
    }
}
