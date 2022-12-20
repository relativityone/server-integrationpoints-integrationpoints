using System.Collections.Generic;
using Atata;
using FluentAssertions;
using Relativity.IntegrationPoints.Tests.Functional.TestsAssertions;
using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using Relativity.IntegrationPoints.Tests.Functional.Web.Extensions;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Web.Models;
using Relativity.Testing.Framework.Web.Navigation;
using KeywordSearch = Relativity.Testing.Framework.Models.KeywordSearch;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal class SavedSearchNativesSyncTestsImplementation : SyncTestsImplementationTemplate
    {
        private int _keywordSearchDocumentsCount = 5;
        private KeywordSearch _keywordSearch;

        public SavedSearchNativesSyncTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture) : base(testsImplementationTestFixture)
        {
        }

        public override IntegrationPointViewPage CreateIntegrationPointViewPage()
        {
            _keywordSearch = new KeywordSearch
            {
                Name = IntegrationPointName,
                SearchCriteria = new CriteriaCollection
                {
                    Conditions = new List<BaseCriteria>
                    {
                        new Criteria { Condition = new CriteriaCondition(new NamedArtifact { Name = "Control Number" }, ConditionOperator.GreaterThanOrEqualTo, "AZIPPER_0007291") },
                        new Criteria { Condition = new CriteriaCondition(new NamedArtifact { Name = "Control Number" }, ConditionOperator.LessThanOrEqualTo, "AZIPPER_0007491") }
                    }
                }
            };

            RelativityFacade.Instance.Resolve<IKeywordSearchService>().Require(TestsImplementationTestFixture.Workspace.ArtifactID, _keywordSearch);

            IntegrationPointListPage integrationPointListPage = Being.On<IntegrationPointListPage>(TestsImplementationTestFixture.Workspace.ArtifactID);
            IntegrationPointEditPage integrationPointEditPage = integrationPointListPage.NewIntegrationPoint.ClickAndGo();
            IntegrationPointViewPage integrationPointViewPage = integrationPointEditPage.CreateSavedSearchToFolderIntegrationPointWithNatives(
                IntegrationPointName,
                DestinationWorkspace,
                _keywordSearch.Name,
                RelativityProviderCopyNativeFiles.PhysicalFiles);

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
            integrationPointViewPage.SummaryPageGeneralTab.MoveExistingDocs.ExpectTo.BeVisibleWithRetries();

            integrationPointViewPage.GetName().ShouldBeEquivalentTo(IntegrationPointName);
            integrationPointViewPage.GetOverwriteMode().ShouldBeEquivalentTo(RelativityProviderOverwrite.AppendOnly);
            integrationPointViewPage.GetExportType().ShouldBeEquivalentTo("Workspace; Natives");
            integrationPointViewPage.GetSourceDetails().ShouldBeEquivalentTo($"Saved Search: {_keywordSearch.Name}");
            integrationPointViewPage.GetSourceWorkspaceName().ShouldBeEquivalentTo(TestsImplementationTestFixture.Workspace.Name);
            integrationPointViewPage.GetSourceRelativityInstance().ShouldBeEquivalentTo("This instance(emttest)");
            integrationPointViewPage.GetTransferredObject().ShouldBeEquivalentTo(IntegrationPointTransferredObjects.Document);
            integrationPointViewPage.GetDestinationWorkspaceName().ShouldBeEquivalentTo(DestinationWorkspace.Name);
            integrationPointViewPage.GetDestinationFolderName().ShouldBeEquivalentTo(DestinationWorkspace.Name);
            integrationPointViewPage.GetMultiSelectOverlayMode().ShouldBeEquivalentTo(FieldOverlayBehavior.UseFieldSettings);
            integrationPointViewPage.GetUseFolderPathInfo().ShouldBeEquivalentTo(RelativityProviderFolderPathInformation.No);
            integrationPointViewPage.GetMoveExistingDocs().ShouldBeEquivalentTo(YesNo.No);

            integrationPointViewPage.SummaryPageGeneralTab.LogErrors.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.HasErrors.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.EmailNotificationRecipients.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.TotalOfDocuments.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.TotalOfNatives.ExpectTo.BeVisibleWithRetries();
            integrationPointViewPage.SummaryPageGeneralTab.CreateSavedSearch.ExpectTo.BeVisibleWithRetries();

            integrationPointViewPage.GetLogErrors().ShouldBeEquivalentTo(YesNo.Yes);
            integrationPointViewPage.GetHasErrors().ShouldBeEquivalentTo(YesNo.No);
            integrationPointViewPage.GetEmailNotificationRecipients().Should().BeNullOrEmpty();
            integrationPointViewPage.GetCreateSavedSearch().ShouldBeEquivalentTo(YesNo.No);
        }

        public override void AssertIntegrationPointJobHistory(IntegrationPointViewPage integrationPointViewPage)
        {
            string expectedDestinationCaseTag = $"This Instance - {DestinationWorkspace.Name} - {DestinationWorkspace.ArtifactID}";
            string expectedSourceCaseTag = $"This Instance - {TestsImplementationTestFixture.Workspace.Name} - {TestsImplementationTestFixture.Workspace.ArtifactID}";
            string expectedSourceJobTag = $"{IntegrationPointName} - {GetJobId(TestsImplementationTestFixture.Workspace.ArtifactID, IntegrationPointName)}";

            List<RelativityObject> sourceDocs = GetDocumentsTagsDataFromSourceWorkspace(TestsImplementationTestFixture.Workspace.ArtifactID);
            List<RelativityObject> destinationDocs = GetDocumentsTagsDataFromDestinationWorkspace(DestinationWorkspace.ArtifactID);

            int transferredItemsCount = integrationPointViewPage.GetTransferredItemsCount(IntegrationPointName);
            int workspaceDocumentCount = RelativityFacade.Instance.Resolve<IDocumentService>().GetAll(DestinationWorkspace.ArtifactID).Length;

            transferredItemsCount.Should().Be(workspaceDocumentCount).And.Be(_keywordSearchDocumentsCount);

            GetCorrectlyTaggedDocumentsCount(sourceDocs, "Relativity Destination Case", expectedDestinationCaseTag).Should().Be(transferredItemsCount);
            GetCorrectlyTaggedDocumentsCount(destinationDocs, "Relativity Source Case", expectedSourceCaseTag).Should().Be(transferredItemsCount);
            GetCorrectlyTaggedDocumentsCount(destinationDocs, "Relativity Source Job", expectedSourceJobTag).Should().Be(transferredItemsCount);

            BillingFlagAssertion.AssertFiles(DestinationWorkspace.ArtifactID, expectBillable: true);
        }
    }
}
