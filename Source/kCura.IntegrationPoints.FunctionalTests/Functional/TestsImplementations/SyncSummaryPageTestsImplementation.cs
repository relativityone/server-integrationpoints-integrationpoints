﻿using System.Collections.Generic;
using Atata;
using FluentAssertions;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using Relativity.IntegrationPoints.Tests.Functional.Web.Extensions;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.Sync.Configuration;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Web.Models;
using Criteria = Relativity.Testing.Framework.Models.Criteria;
using CriteriaCollection = Relativity.Testing.Framework.Models.CriteriaCollection;
using CriteriaCondition = Relativity.Testing.Framework.Models.CriteriaCondition;
using KeywordSearch = Relativity.Testing.Framework.Models.KeywordSearch;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal class SyncSummaryPageTestsImplementation : SyncTestsImplementationBase
    {
        public SyncSummaryPageTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture) : base(testsImplementationTestFixture)
        {
        }

        public override void OnSetUpFixture()
        {
        }


        public void SavedSearchNativesAndMetadataSummaryPage()
        {
            RelativityFacade.Instance.ImportDocumentsFromCsv(TestsImplementationTestFixture.Workspace,
                LoadFilesGenerator.GetOrCreateNativesLoadFile());

            string integrationPointName = nameof(SavedSearchNativesAndMetadataSummaryPage);

            KeywordSearch keywordSearch = new KeywordSearch
            {
                Name = integrationPointName,
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

            IntegrationPointViewPage CreateIntegrationPointViewPage(Workspace destinationWorkspace,
                IntegrationPointEditPage integrationPointEditPage)
            {
                IntegrationPointViewPage integrationPointViewPage =
                    integrationPointEditPage.CreateSavedSearchToFolderIntegrationPointWithNatives(integrationPointName,
                        destinationWorkspace, keywordSearch.Name, RelativityProviderCopyNativeFiles.PhysicalFiles);

                return integrationPointViewPage;
            }

            void Assert(Workspace destinationWorkspace,
                IntegrationPointViewPage integrationPointViewPage)
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

                integrationPointViewPage.GetName().ShouldBeEquivalentTo(integrationPointName);
                integrationPointViewPage.GetOverwriteMode().ShouldBeEquivalentTo(RelativityProviderOverwrite.AppendOnly);
                integrationPointViewPage.GetExportType().ShouldBeEquivalentTo("Workspace; Natives");
                integrationPointViewPage.GetSourceDetails().ShouldBeEquivalentTo($"Saved Search: {keywordSearch.Name}");
                integrationPointViewPage.GetSourceWorkspaceName().ShouldBeEquivalentTo(TestsImplementationTestFixture.Workspace.Name);
                integrationPointViewPage.GetSourceRelativityInstance().ShouldBeEquivalentTo("This instance(emttest)");
                integrationPointViewPage.GetTransferredObject().ShouldBeEquivalentTo(ArtifactType.Document);
                integrationPointViewPage.GetDestinationWorkspaceName().ShouldBeEquivalentTo(destinationWorkspace.Name);
                integrationPointViewPage.GetDestinationFolderName().ShouldBeEquivalentTo(destinationWorkspace.Name);
                integrationPointViewPage.GetMultiSelectOverlayMode().ShouldBeEquivalentTo(FieldOverlayBehavior.UseFieldSettings);
                integrationPointViewPage.GetUseFolderPathInfo().ShouldBeEquivalentTo(RelativityProviderFolderPathInformation.No);
                integrationPointViewPage.GetMoveExistingDocs().ShouldBeEquivalentTo(YesNo.No);

                #endregion

                #region 2nd column

                const int keywordSearchDocumentsCount = 5;
                integrationPointViewPage.SummaryPageGeneralTab.LogErrors.ExpectTo.BeVisible();
                integrationPointViewPage.SummaryPageGeneralTab.HasErrors.ExpectTo.BeVisible();
                integrationPointViewPage.SummaryPageGeneralTab.EmailNotificationRecipients.ExpectTo.BeVisible();
                integrationPointViewPage.SummaryPageGeneralTab.TotalOfDocuments.ExpectTo.BeVisible();
                integrationPointViewPage.SummaryPageGeneralTab.TotalOfNatives.ExpectTo.BeVisible();
                integrationPointViewPage.SummaryPageGeneralTab.CreateSavedSearch.ExpectTo.BeVisible();

                integrationPointViewPage.GetLogErrors().ShouldBeEquivalentTo(YesNo.Yes);
                integrationPointViewPage.GetHasErrors().ShouldBeEquivalentTo(YesNo.No);
                integrationPointViewPage.GetEmailNotificationRecipients().Should().BeNullOrEmpty();
                integrationPointViewPage.GetTotalDocuments().ShouldBeEquivalentTo(keywordSearchDocumentsCount);
                integrationPointViewPage.GetTotalNatives().ShouldBeEquivalentTo($"{keywordSearchDocumentsCount} (12.52 KB)");
                integrationPointViewPage.GetCreateSavedSearch().ShouldBeEquivalentTo(YesNo.No);

                #endregion
            }

            ExecuteSavedSearchTest(keywordSearch, CreateIntegrationPointViewPage, Assert);
        }

        public void SavedSearchImagesSummaryPage()
        {
            // Arrange
            string integrationPointName = nameof(SavedSearchImagesSummaryPage);

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

            const int imagesCount = 10;
            string testDataPath = LoadFilesGenerator.GetOrCreateNativesOptLoadFile();
            RelativityFacade.Instance.ImportImages(TestsImplementationTestFixture.Workspace,
                testDataPath, imageImportOptions, imagesCount);

            KeywordSearch keywordSearch = new KeywordSearch
            {
                Name = nameof(SavedSearchImagesSummaryPage),
                SearchCriteria = new CriteriaCollection
                {
                    Conditions = new List<BaseCriteria>
                    {
                        new Criteria
                        {
                            Condition = new CriteriaCondition(new NamedArtifact { Name = "Has Images" }, ConditionOperator.AnyOfThese, new List<int> { 1034243 })
                        }
                    }
                }
            };

            IntegrationPointViewPage CreateIntegrationPointViewPage(Workspace destinationWorkspace,
                IntegrationPointEditPage integrationPointEditPage)
            {
                IntegrationPointViewPage integrationPointViewPage = integrationPointEditPage.CreateSavedSearchToFolderIntegrationPointWithImages(integrationPointName,
                    destinationWorkspace, keywordSearch.Name);

                return integrationPointViewPage;
            }

            void Assert(Workspace destinationWorkspace, IntegrationPointViewPage integrationPointViewPage)
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

                integrationPointViewPage.GetName().ShouldBeEquivalentTo(integrationPointName);
                integrationPointViewPage.GetOverwriteMode().ShouldBeEquivalentTo(RelativityProviderOverwrite.AppendOverlay);
                integrationPointViewPage.GetExportType().ShouldBeEquivalentTo("Workspace; ImagesNatives");
                integrationPointViewPage.GetSourceDetails().ShouldBeEquivalentTo($"Saved Search: {keywordSearch.Name}");
                integrationPointViewPage.GetSourceWorkspaceName().ShouldBeEquivalentTo(TestsImplementationTestFixture.Workspace.Name);
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
                integrationPointViewPage.GetTotalDocuments().ShouldBeEquivalentTo(imagesCount);
                integrationPointViewPage.GetTotalImages().ShouldBeEquivalentTo($"{imagesCount} (0 Bytes)");
                integrationPointViewPage.GetCreateSavedSearch().ShouldBeEquivalentTo(YesNo.No);

                #endregion
            }

            ExecuteSavedSearchTest(keywordSearch, CreateIntegrationPointViewPage, Assert);
        }

        public void ProductionImagesGoldFlow()
        {

            ExecuteProductionTest();
        }
    }
}