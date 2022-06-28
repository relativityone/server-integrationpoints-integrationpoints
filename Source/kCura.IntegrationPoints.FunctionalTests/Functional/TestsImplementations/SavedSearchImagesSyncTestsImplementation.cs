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
            RelativityFacade.Instance.ImportImages(TestsImplementationTestFixture.Workspace,
                testDataPath, imageImportOptions, _imagesCount);

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
            IntegrationPointViewPage integrationPointViewPage = integrationPointEditPage.CreateSavedSearchToFolderIntegrationPointWithNatives(IntegrationPointName,
                        DestinationWorkspace, _keywordSearch.Name, RelativityProviderCopyNativeFiles.PhysicalFiles);

            return integrationPointViewPage;
        }

        public override void AssertIntegrationPointSummaryPageGeneralTab(IntegrationPointViewPage integrationPointViewPage)
        {
            #region 1st column

            integrationPointViewPage.SummaryPageGeneralTab.Name.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.Overwrite.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.ExportType.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.SourceDetails.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.SourceWorkspace.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.SourceRelativityInstance.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.TransferedObject.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.DestinationWorkspace.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.DestinationFolder.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.MultiSelectOverlay.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.UseFolderPathInfo.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.ImagePrecedence.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.CopyFilesToRepository.ExpectTo.BeVisible();

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

            #endregion

            #region 2nd column

            integrationPointViewPage.SummaryPageGeneralTab.LogErrors.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.HasErrors.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.EmailNotificationRecipients.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.TotalOfDocuments.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.TotalOfImages.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.CreateSavedSearch.ExpectTo.BeVisible();

            integrationPointViewPage.GetLogErrors().ShouldBeEquivalentTo(YesNo.Yes);
            integrationPointViewPage.GetHasErrors().ShouldBeEquivalentTo(YesNo.No);
            integrationPointViewPage.GetEmailNotificationRecipients().ShouldBeEquivalentTo("Adler@Sieben.com");
            integrationPointViewPage.GetTotalDocuments().ShouldBeEquivalentTo(_imagesCount);
            integrationPointViewPage.GetTotalImages().ShouldBeEquivalentTo($"{_imagesCount} (0 Bytes)");
            integrationPointViewPage.GetCreateSavedSearch().ShouldBeEquivalentTo(YesNo.No);

            #endregion
        }

        public override void AssertIntegrationPointJobHistory(IntegrationPointViewPage integrationPointViewPage)
        {
            throw new System.NotImplementedException();
        }

    }
}
