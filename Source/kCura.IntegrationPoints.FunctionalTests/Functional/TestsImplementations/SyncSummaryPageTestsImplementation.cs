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
            IKeywordSearchService keywordSearchService = RelativityFacade.Instance.Resolve<IKeywordSearchService>();
            KeywordSearch keyWordSearch =
                keywordSearchService.Require(_testsImplementationTestFixture.Workspace.ArtifactID, keywordSearch);
            //IObjectManager objectManager = RelativityFacade.Instance.Resolve<IObjectManager>();

            // Act
            IntegrationPointListPage integrationPointListPage =
                Being.On<IntegrationPointListPage>(_testsImplementationTestFixture.Workspace.ArtifactID);
            IntegrationPointEditPage integrationPointEditPage =
                integrationPointListPage.NewIntegrationPoint.ClickAndGo();

            IntegrationPointViewPage integrationPointViewPage =
                integrationPointEditPage.CreateSavedSearchToFolderIntegrationPointWithNatives(integrationPointName,
                    destinationWorkspace, keywordSearch.Name, RelativityProviderCopyNativeFiles.PhysicalFiles);

            // Assert

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

            integrationPointViewPage.GetName().ShouldBeEquivalentTo(integrationPointName);
            integrationPointViewPage.GetOverwriteMode().ShouldBeEquivalentTo(RelativityProviderOverwrite.AppendOnly);
            integrationPointViewPage.GetExportType().ShouldBeEquivalentTo("Workspace; Natives");
            integrationPointViewPage.GetSourceDetails().ShouldBeEquivalentTo($"Saved Search: {keywordSearch.Name}");
            integrationPointViewPage.GetSourceWorkspaceName()
                .ShouldBeEquivalentTo(_testsImplementationTestFixture.Workspace.Name);
            integrationPointViewPage.GetSourceRelativityInstance().ShouldBeEquivalentTo("This instance(emttest)");
            integrationPointViewPage.GetTransferredObject().ShouldBeEquivalentTo(ArtifactType.Document);
            integrationPointViewPage.GetDestinationWorkspaceName().ShouldBeEquivalentTo(destinationWorkspace.Name);
            integrationPointViewPage.GetDestinationFolderName().ShouldBeEquivalentTo(destinationWorkspace.Name);
            integrationPointViewPage.GetMultiSelectOverlayMode()
                .ShouldBeEquivalentTo(FieldOverlayBehavior.UseFieldSettings);
            integrationPointViewPage.GetUseFolderPathInfo()
                .ShouldBeEquivalentTo(RelativityProviderFolderPathInformation.No);
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
            integrationPointViewPage.GetTotalDocuments().ShouldBeEquivalentTo(5);
            integrationPointViewPage.GetTotalNatives().ShouldBeEquivalentTo("5 (12.52 KB)");
            integrationPointViewPage.GetCreateSavedSearch().ShouldBeEquivalentTo(YesNo.No);

            #endregion

            //bool run = true;
            //while (run)
            //{
            //    try
            //    {
            //QueryRequest queryRequest = new QueryRequest
            //{
            //    ObjectType = new ObjectTypeRef() { ArtifactTypeID = (int)ArtifactType.Search },
            //    Condition = $"'Artifact ID' == {keyWordSearch.ArtifactID}",
            //    Fields = new[] { new FieldRef { Name = "Name" }, new FieldRef { Name = "Owner" } }
            //};

            //ISavedSearchQueryRepository savedSearchQueryRepository = RelativityFacade.Instance.Resolve<ISavedSearchQueryRepository>();
            //SavedSearchDTO queryRepoSavedSearchDTO = savedSearchQueryRepository.RetrieveSavedSearch(keyWordSearch.ArtifactID);

            //RelativityObject savedSearchObject = objectManager.Query(queryRequest).FirstOrDefault();
            //SavedSearchDTO savedSearchDTO = savedSearchObject?.ToSavedSearchDTO();

            //ISearchContainerManager searchContainerManager = RelativityFacade.Instance.Resolve<ISearchContainerManager>();
            //SearchContainer result = searchContainerManager.ReadSingleAsync(_testsImplementationTestFixture.Workspace.ArtifactID,
            //    queryRepoSavedSearchDTO.ParentContainerId).GetAwaiter().GetResult();

            //var searchManager = RelativityFacade.Instance.Resolve<ISearchService>();
            //searchManager.


            //        run = false;
            //    }
            //    catch (Exception)
            //    {
            //    }
            //}
        }

        public void SavedSearchImagesSummaryPage()
        {
            // Arrange
            _testsImplementationTestFixture.LoginAsStandardUser();

            string integrationPointName = nameof(SavedSearchImagesSummaryPage);

            Workspace destinationWorkspace = CreateDestinationWorkspace();
            KeywordSearch keywordSearch = new KeywordSearch
            {
                Name = nameof(SavedSearchImagesSummaryPage),
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
            KeywordSearch keyWordSearch = keywordSearchService.Require(_testsImplementationTestFixture.Workspace.ArtifactID, keywordSearch);
            //IObjectManager objectManager = RelativityFacade.Instance.Resolve<IObjectManager>();

            // Act
            IntegrationPointListPage integrationPointListPage = Being.On<IntegrationPointListPage>(_testsImplementationTestFixture.Workspace.ArtifactID);
            IntegrationPointEditPage integrationPointEditPage = integrationPointListPage.NewIntegrationPoint.ClickAndGo();

            IntegrationPointViewPage integrationPointViewPage = integrationPointEditPage.CreateSavedSearchToFolderIntegrationPointWithImages(integrationPointName,
                destinationWorkspace, keywordSearch.Name);

            // Assert
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

            integrationPointViewPage.GetName().ShouldBeEquivalentTo(integrationPointName);
            integrationPointViewPage.GetOverwriteMode().ShouldBeEquivalentTo(RelativityProviderOverwrite.AppendOverlay);
            integrationPointViewPage.GetExportType().ShouldBeEquivalentTo("Workspace; ImagesNatives");
            integrationPointViewPage.GetSourceDetails().ShouldBeEquivalentTo($"Saved Search: {keywordSearch.Name}");
            integrationPointViewPage.GetSourceWorkspaceName().ShouldBeEquivalentTo(_testsImplementationTestFixture.Workspace.Name);
            integrationPointViewPage.GetSourceRelativityInstance().ShouldBeEquivalentTo("This instance(emttest)");
            integrationPointViewPage.GetTransferredObject().ShouldBeEquivalentTo(ArtifactType.Document);
            integrationPointViewPage.GetDestinationWorkspaceName().ShouldBeEquivalentTo(destinationWorkspace.Name);
            integrationPointViewPage.GetDestinationFolderName().ShouldBeEquivalentTo(destinationWorkspace.Name);
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
            integrationPointViewPage.GetEmailNotificationRecipients().Should().BeNullOrEmpty();
            integrationPointViewPage.GetTotalDocuments().ShouldBeEquivalentTo(5);
            integrationPointViewPage.GetTotalImages().ShouldBeEquivalentTo("0 (0 Bytes)");
            integrationPointViewPage.GetCreateSavedSearch().ShouldBeEquivalentTo(YesNo.No);

            #endregion
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