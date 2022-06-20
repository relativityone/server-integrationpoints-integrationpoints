﻿using System;
using System.Collections.Generic;
using Atata;
using FluentAssertions;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.IntegrationPoints.Tests.Common.Extensions;
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
using Criteria = Relativity.Testing.Framework.Models.Criteria;
using CriteriaCollection = Relativity.Testing.Framework.Models.CriteriaCollection;
using CriteriaCondition = Relativity.Testing.Framework.Models.CriteriaCondition;
using KeywordSearch = Relativity.Testing.Framework.Models.KeywordSearch;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal class SyncSummaryPageTestsImplementation
    {
        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;
        private readonly Dictionary<string, Workspace> _destinationWorkspaces = new Dictionary<string, Workspace>();

        public SyncSummaryPageTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
        {
            _testsImplementationTestFixture = testsImplementationTestFixture;
        }

        public void OnSetUpFixture()
        {
            RelativityFacade.Instance.ImportDocumentsFromCsv(_testsImplementationTestFixture.Workspace,
                LoadFilesGenerator.GetOrCreateNativesLoadFile());
        }

        public void OnTearDownFixture()
        {
            foreach (KeyValuePair<string, Workspace> destinationWorkspace in _destinationWorkspaces)
            {
                RelativityFacade.Instance.DeleteWorkspace(destinationWorkspace.Value);
            }
        }

        public void SavedSearchNativesAndMetadataSummaryPage()
        {
            // Arrange
            _testsImplementationTestFixture.LoginAsStandardUser();

            string integrationPointName = nameof(SavedSearchNativesAndMetadataSummaryPage);

            Workspace destinationWorkspace = CreateDestinationWorkspace();
            KeywordSearch keywordSearch = new KeywordSearch
            {
                Name = nameof(SavedSearchNativesAndMetadataSummaryPage),
                SearchCriteria = new CriteriaCollection
                {
                    Conditions = new List<BaseCriteria>
                    {
                        new Criteria
                        {
                            Condition = new CriteriaCondition(new NamedArtifact { Name = "Control Number" }, ConditionOperator.GreaterThanOrEqualTo, "AZIPPER_0007291")
                        },
                        new Criteria
                        {
                            Condition = new CriteriaCondition(new NamedArtifact { Name = "Control Number" }, ConditionOperator.LessThanOrEqualTo, "AZIPPER_0007491")
                        }
                    }
                }
            };
            IKeywordSearchService keywordSearchService = RelativityFacade.Instance.Resolve<IKeywordSearchService>();
            IRelativityObjectManager objectManager = RelativityFacade.Instance.Resolve<IRelativityObjectManager>();
            KeywordSearch keyWordSearch = keywordSearchService.Require(_testsImplementationTestFixture.Workspace.ArtifactID, keywordSearch);

            // Act
            IntegrationPointListPage integrationPointListPage = Being.On<IntegrationPointListPage>(_testsImplementationTestFixture.Workspace.ArtifactID);
            IntegrationPointEditPage integrationPointEditPage = integrationPointListPage.NewIntegrationPoint.ClickAndGo();

            IntegrationPointViewPage integrationPointViewPage = integrationPointEditPage.CreateSavedSearchToFolderIntegrationPoint(integrationPointName,
                destinationWorkspace, keywordSearch.Name, RelativityProviderCopyNativeFiles.PhysicalFiles);

            // Assert
            #region 1st column

            integrationPointViewPage.SummaryPageGeneralTab.Name.WaitTo.Within(5).BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.Overwrite.WaitTo.Within(5).BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.ExportType.WaitTo.Within(5).BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.SourceDetails.WaitTo.Within(5).BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.SourceWorkspace.WaitTo.Within(5).BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.SourceRelativityInstance.WaitTo.Within(5).BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.TransferedObject.WaitTo.Within(5).BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.DestinationWorkspace.WaitTo.Within(5).BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.DestinationFolder.WaitTo.Within(5).BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.MultiSelectOverlay.WaitTo.Within(5).BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.UseFolderPathInfo.WaitTo.Within(5).BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.MoveExistingDocs.WaitTo.Within(5).BeVisible();

            integrationPointViewPage.GetName().ShouldBeEquivalentTo(integrationPointName);
            integrationPointViewPage.GetOverwriteMode().ShouldBeEquivalentTo(RelativityProviderOverwrite.AppendOnly);
            integrationPointViewPage.GetExportType().ShouldBeEquivalentTo("Workspace; Natives");
            integrationPointViewPage.GetSourceDetails().ShouldBeEquivalentTo($"Saved Search: {keywordSearch.Name}");
            integrationPointViewPage.GetSourceWorkspaceName().ShouldBeEquivalentTo(_testsImplementationTestFixture.Workspace.Name);
            integrationPointViewPage.GetSourceRelativityInstance().ShouldBeEquivalentTo("This instance(emttest)");
            integrationPointViewPage.GetTransferredObject().ShouldBeEquivalentTo(ArtifactType.Document);
            integrationPointViewPage.GetDestinationWorkspaceName().ShouldBeEquivalentTo(destinationWorkspace.Name);
            integrationPointViewPage.GetDestinationFolderName().ShouldBeEquivalentTo(destinationWorkspace.Name);
            integrationPointViewPage.GetMultiSelectOverlayMode().ShouldBeEquivalentTo(FieldOverlayBehavior.UseFieldSettings);
            integrationPointViewPage.GetUseFolderPathInfo().ShouldBeEquivalentTo(RelativityProviderFolderPathInformation.No);
            integrationPointViewPage.GetMoveExistingDocs().ShouldBeEquivalentTo(YesNo.No);

            #endregion

            integrationPointViewPage.SummaryPageGeneralTab.LogErrors.WaitTo.Within(5).BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.HasErrors.WaitTo.Within(5).BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.EmailNotificationRecipients.WaitTo.Within(5).BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.TotalOfDocuments.WaitTo.Within(5).BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.TotalOfNatives.WaitTo.Within(5).BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.TotalOfImages.WaitTo.Within(5).Not.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.CreateSavedSearch.WaitTo.Within(5).BeVisible();

            integrationPointViewPage.GetLogErrors().ShouldBeEquivalentTo(YesNo.Yes);
            integrationPointViewPage.GetHasErrors().ShouldBeEquivalentTo(YesNo.No);
            integrationPointViewPage.GetEmailNotificationRecipients().Should().BeNullOrEmpty();
            integrationPointViewPage.GetTotalDocuments().ShouldBeEquivalentTo(5);
            integrationPointViewPage.GetTotalNatives().ShouldBeEquivalentTo("5 (12.52 KB)");
            integrationPointViewPage.GetCreateSavedSearch().ShouldBeEquivalentTo(YesNo.No);

            
        }

        private Workspace CreateDestinationWorkspace()
        {
            string workspaceName = $"Sync - Dest {Guid.NewGuid()}";

            Workspace workspace = RelativityFacade.Instance.CreateWorkspace(workspaceName, _testsImplementationTestFixture.Workspace.Name);

            _destinationWorkspaces.Add(workspaceName, workspace);

            workspace.InstallLegalHold();

            return workspace;
        }
    }
}