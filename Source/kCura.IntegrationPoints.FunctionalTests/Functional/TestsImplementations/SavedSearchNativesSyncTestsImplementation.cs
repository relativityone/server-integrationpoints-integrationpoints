﻿using System.Collections.Generic;
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
                        new Criteria
                        {
                            Condition = new CriteriaCondition(new NamedArtifact { Name = "Control Number" },
                                ConditionOperator.GreaterThanOrEqualTo, "AZIPPER_0007291")
                        },
                        new Criteria
                        {
                            Condition = new CriteriaCondition(new NamedArtifact { Name = "Control Number" },
                                ConditionOperator.LessThanOrEqualTo, "AZIPPER_0007491")
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
            integrationPointViewPage.SummaryPageGeneralTab.MoveExistingDocs.ExpectTo.BeVisible();

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

            #endregion

            #region 2nd column

            integrationPointViewPage.SummaryPageGeneralTab.LogErrors.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.HasErrors.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.EmailNotificationRecipients.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.TotalOfDocuments.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.TotalOfNatives.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.CreateSavedSearch.ExpectTo.BeVisible();

            integrationPointViewPage.GetLogErrors().ShouldBeEquivalentTo(YesNo.Yes);
            integrationPointViewPage.GetHasErrors().ShouldBeEquivalentTo(YesNo.No);
            integrationPointViewPage.GetEmailNotificationRecipients().Should().BeNullOrEmpty();
            integrationPointViewPage.GetTotalDocuments().ShouldBeEquivalentTo(_keywordSearchDocumentsCount);
            integrationPointViewPage.GetTotalNatives().ShouldBeEquivalentTo($"{_keywordSearchDocumentsCount} (12.52 KB)");
            integrationPointViewPage.GetCreateSavedSearch().ShouldBeEquivalentTo(YesNo.No);

            #endregion
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