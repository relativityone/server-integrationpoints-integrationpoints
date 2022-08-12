using System.Collections.Generic;
using Atata;
using FluentAssertions;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using Relativity.IntegrationPoints.Tests.Functional.Web.Extensions;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.Sync.Configuration;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Web.Models;
using Relativity.Testing.Framework.Web.Navigation;
using KeywordSearch = Relativity.Testing.Framework.Models.KeywordSearch;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal class SavedSearchImagesSyncTestsImplementation : SyncTestsImplementationTemplate
    {
        private KeywordSearch _keywordSearch;
        private int _imagesCount = 10;

        public SavedSearchImagesSyncTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture) : base(testsImplementationTestFixture)
        {

        }

        public override IntegrationPointViewPage CreateIntegrationPointViewPage()
        {
            IntegrationPointName = nameof(SavedSearchImagesSyncTestsImplementation);

            ImageImportOptions imageImportOptions = new ImageImportOptions
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

            string testDataPath = LoadFilesGenerator.GetOrCreateNativesOptLoadFile();
            RelativityFacade.Instance.ImportImages(TestsImplementationTestFixture.Workspace, testDataPath, imageImportOptions, _imagesCount);

            const int hasImagesYesArtifactId = 1034243;
            _keywordSearch = new KeywordSearch
            {
                Name = IntegrationPointName,
                SearchCriteria = new CriteriaCollection
                {
                    Conditions = new List<BaseCriteria>
                    {
                        new Criteria
                        {
                            Condition = new CriteriaCondition(new NamedArtifact { Name = "Has Images" }, ConditionOperator.AnyOfThese, new List<int> { hasImagesYesArtifactId })
                        }
                    }
                }
            };

            RelativityFacade.Instance.Resolve<IKeywordSearchService>().Require(TestsImplementationTestFixture.Workspace.ArtifactID, _keywordSearch);

            IntegrationPointListPage integrationPointListPage = Being.On<IntegrationPointListPage>(TestsImplementationTestFixture.Workspace.ArtifactID);
            IntegrationPointEditPage integrationPointEditPage = integrationPointListPage.NewIntegrationPoint.ClickAndGo();
            IntegrationPointViewPage integrationPointViewPage = integrationPointEditPage.CreateSavedSearchToFolderIntegrationPointWithImages(IntegrationPointName,
                    DestinationWorkspace, _keywordSearch.Name);

            return integrationPointViewPage;
        }

        public override void AssertIntegrationPointSummaryPageGeneralTab(IntegrationPointViewPage integrationPointViewPage)
        {
            integrationPointViewPage.SummaryPageGeneralTab.Name.ExpectTo.BeVisibleWithRetries(3);
            integrationPointViewPage.SummaryPageGeneralTab.Overwrite.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.ExportType.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.SourceDetails.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.SourceWorkspace.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.SourceRelativityInstance.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.TransferedObject.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.DestinationWorkspace.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.DestinationFolder.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.MultiSelectOverlay.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.UseFolderPathInfo.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.ImagePrecedence.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.CopyFilesToRepository.ExpectTo.BeVisibleWithRetries();

            integrationPointViewPage.GetName().ShouldBeEquivalentTo(IntegrationPointName);
            integrationPointViewPage.GetOverwriteMode().ShouldBeEquivalentTo(RelativityProviderOverwrite.AppendOverlay);
            integrationPointViewPage.GetExportType().ShouldBeEquivalentTo("Workspace; ImagesNatives");
            integrationPointViewPage.GetSourceDetails().ShouldBeEquivalentTo($"Saved Search: {_keywordSearch.Name}");
            integrationPointViewPage.GetSourceWorkspaceName().ShouldBeEquivalentTo(TestsImplementationTestFixture.Workspace.Name);
            integrationPointViewPage.GetSourceRelativityInstance().ShouldBeEquivalentTo("This instance(emttest)");
            integrationPointViewPage.GetTransferredObject().ShouldBeEquivalentTo(IntegrationPointTransferredObjects.Document);
            integrationPointViewPage.GetDestinationWorkspaceName().ShouldBeEquivalentTo(DestinationWorkspace.Name);
            integrationPointViewPage.GetDestinationFolderName().ShouldBeEquivalentTo(DestinationWorkspace.Name);
            integrationPointViewPage.GetMultiSelectOverlayMode().ShouldBeEquivalentTo(FieldOverlayBehavior.MergeValues);
            integrationPointViewPage.GetUseFolderPathInfo().ShouldBeEquivalentTo(RelativityProviderFolderPathInformation.No);
            integrationPointViewPage.GetImagePrecedence().ShouldBeEquivalentTo(RelativityProviderImagePrecedence.OriginalImages);
            integrationPointViewPage.GetCopyFilesToRepository().ShouldBeEquivalentTo(YesNo.Yes);

            integrationPointViewPage.SummaryPageGeneralTab.LogErrors.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.HasErrors.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.EmailNotificationRecipients.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.TotalOfDocuments.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.TotalOfImages.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.CreateSavedSearch.ExpectTo.BeVisibleWithRetries();

            integrationPointViewPage.GetLogErrors().ShouldBeEquivalentTo(YesNo.Yes);
            integrationPointViewPage.GetHasErrors().ShouldBeEquivalentTo(YesNo.No);
            integrationPointViewPage.GetEmailNotificationRecipients().ShouldBeEquivalentTo("Adler@Sieben.com");
            integrationPointViewPage.GetTotalDocuments().ShouldBeEquivalentTo(_imagesCount);
            integrationPointViewPage.GetTotalImages().ShouldBeEquivalentTo($"{_imagesCount} (0 Bytes)");
            integrationPointViewPage.GetCreateSavedSearch().ShouldBeEquivalentTo(YesNo.No);
        }

        public override void AssertIntegrationPointJobHistory(IntegrationPointViewPage integrationPointViewPage)
        {
            throw new System.NotImplementedException();
        }

    }
}
