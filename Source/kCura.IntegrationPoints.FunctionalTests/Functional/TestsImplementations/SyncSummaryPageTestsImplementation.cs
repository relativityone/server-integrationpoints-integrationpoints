using System;
using System.Collections.Generic;
using Atata;
using FluentAssertions;
using Relativity.IntegrationPoints.Tests.Common.Extensions;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using Relativity.IntegrationPoints.Tests.Functional.Web.Extensions;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Web.Navigation;

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
            RelativityFacade.Instance.Resolve<IKeywordSearchService>().Require(_testsImplementationTestFixture.Workspace.ArtifactID, keywordSearch);

            // Act
            IntegrationPointListPage integrationPointListPage = Being.On<IntegrationPointListPage>(_testsImplementationTestFixture.Workspace.ArtifactID);
            IntegrationPointEditPage integrationPointEditPage = integrationPointListPage.NewIntegrationPoint.ClickAndGo();

            IntegrationPointViewPage integrationPointViewPage = integrationPointEditPage.CreateSavedSearchToFolderIntegrationPoint(integrationPointName,
                destinationWorkspace, keywordSearch.Name, RelativityProviderCopyNativeFiles.PhysicalFiles);

            // Assert
            integrationPointViewPage.SummaryPageGeneralTab.Name.IsVisible;
            integrationPointViewPage.GetOverwriteMode.ShouldBeEquivalentTo(integrationPointEditPage.Type);
            integrationPointViewPage.GetExportType.ShouldBeEquivalentTo();
            integrationPointViewPage.GetSourceDetails.ShouldBeEquivalentTo($"Saved Search; {keywordSearch.Name}");
            integrationPointViewPage.GetSourceWorkspace.ShouldBeEquivalentTo();
            integrationPointViewPage.GetSourceRelInstance.ShouldBeEquivalentTo();
            integrationPointViewPage.GetTransferredObject.ShouldBeEquivalentTo();
            integrationPointViewPage.GetDestinationWorkspace.ShouldBeEquivalentTo(destinationWorkspace.Name);
            integrationPointViewPage.GetDestinationFolder.ShouldBeEquivalentTo();
            integrationPointViewPage.GetMultiSelectOverlay.ShouldBeEquivalentTo();
            integrationPointViewPage.GetUseFolderPathInfo.ShouldBeEquivalentTo();
            integrationPointViewPage.GetMoveExistingDocs.ShouldBeEquivalentTo();

            integrationPointViewPage.GetName.ShouldBeEquivalentTo(integrationPointName);
            integrationPointViewPage.GetOverwriteMode.ShouldBeEquivalentTo(integrationPointEditPage.Type);
            integrationPointViewPage.GetExportType.ShouldBeEquivalentTo();
            integrationPointViewPage.GetSourceDetails.ShouldBeEquivalentTo($"Saved Search; {keywordSearch.Name}");
            integrationPointViewPage.GetSourceWorkspace.ShouldBeEquivalentTo();
            integrationPointViewPage.GetSourceRelInstance.ShouldBeEquivalentTo();
            integrationPointViewPage.GetTransferredObject.ShouldBeEquivalentTo();
            integrationPointViewPage.GetDestinationWorkspace.ShouldBeEquivalentTo(destinationWorkspace.Name);
            integrationPointViewPage.GetDestinationFolder.ShouldBeEquivalentTo();
            integrationPointViewPage.GetMultiSelectOverlay.ShouldBeEquivalentTo();
            integrationPointViewPage.GetUseFolderPathInfo.ShouldBeEquivalentTo();
            integrationPointViewPage.GetMoveExistingDocs.ShouldBeEquivalentTo();
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