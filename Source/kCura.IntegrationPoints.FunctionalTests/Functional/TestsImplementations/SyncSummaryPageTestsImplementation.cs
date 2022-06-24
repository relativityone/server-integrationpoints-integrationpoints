using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Atata;
using FluentAssertions;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using Relativity.IntegrationPoints.Tests.Functional.Web.Extensions;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.Shared.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api;
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
    internal class SyncSummaryPageTestsImplementation : SyncTestsImplementationBase
    {
        public SyncSummaryPageTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture) : base(testsImplementationTestFixture)
        {
        }

        public void SavedSearchNativesAndMetadataSummaryPage()
        {
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
                integrationPointViewPage.GetTransferredObject().ShouldBeEquivalentTo(IntegrationPointTransferredObjects.Document);
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


            const int hasImagesYesArtifactId = 1034243;
            KeywordSearch keywordSearch = new KeywordSearch
            {
                Name = nameof(SavedSearchImagesSummaryPage),
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
                integrationPointViewPage.GetTransferredObject().ShouldBeEquivalentTo(IntegrationPointTransferredObjects.Document);
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

        public void ProductionImagesSummaryPage()
        {
            TestsImplementationTestFixture.LoginAsStandardUser();

            string integrationPointName = $"{nameof(ProductionImagesSummaryPage)} - {Guid.NewGuid()}";

            Workspace destinationWorkspace = CreateDestinationWorkspace();

            KeywordSearch keywordSearch = RelativityFacade.Instance.Resolve<IKeywordSearchService>().Require(TestsImplementationTestFixture.Workspace.ArtifactID, new KeywordSearch
            {
                Name = nameof(ProductionImagesSummaryPage),
                SearchCriteria = new CriteriaCollection
                {
                    Conditions = new List<BaseCriteria>
                    {
                        new Criteria
                        {
                            Condition = new CriteriaCondition(new NamedArtifact { Name = "Has Native" }, ConditionOperator.Is, true)
                        },
                        new Criteria
                        {
                            Condition = new CriteriaCondition(new NamedArtifact { Name = "Control Number" }, ConditionOperator.GreaterThanOrEqualTo, "AZIPPER_0007494")
                        },
                        new Criteria
                        {
                            Condition = new CriteriaCondition(new NamedArtifact { Name = "Control Number" }, ConditionOperator.LessThanOrEqualTo, "AZIPPER_0007748")
                        }
                    }
                }
            });

            ProductionPlaceholder productionPlaceholder = new ProductionPlaceholder
            {
                Name = nameof(ProductionImagesSummaryPage),
                PlaceholderType = PlaceholderType.Image,
                FileName = "Image",
                FileData = Convert.ToBase64String(File.ReadAllBytes(DataFiles.PLACEHOLDER_IMAGE_PATH))
            };
            RelativityFacade.Instance.Resolve<IProductionPlaceholderService>().Create(TestsImplementationTestFixture.Workspace.ArtifactID, productionPlaceholder);


            Testing.Framework.Models.Production production = new Testing.Framework.Models.Production
            {
                Name = nameof(ProductionImagesSummaryPage),
                Numbering = new ProductionNumbering
                {
                    NumberingType = NumberingType.OriginalImage,
                    BatesPrefix = "Prefix",
                    BatesSuffix = "Suffix",
                    NumberOfDigitsForDocumentNumbering = 7,
                    BatesStartNumber = 6,
                    AttachmentRelationalField = new NamedArtifact()
                },

                DataSources = new List<ProductionDataSource>
                {
                    new ProductionDataSource
                    {
                        Name = nameof(ProductionImagesSummaryPage),
                        ProductionType = Testing.Framework.Models.ProductionType.ImagesAndNatives,
                        SavedSearch = new NamedArtifact
                        {
                            ArtifactID = keywordSearch.ArtifactID
                        },
                        UseImagePlaceholder = UseImagePlaceholderOption.AlwaysUseImagePlaceholder,
                        Placeholder = new NamedArtifact
                        {
                            ArtifactID = productionPlaceholder.ArtifactID
                        },
                    }
                }
            };
            RelativityFacade.Instance.ProduceProduction(TestsImplementationTestFixture.Workspace, production);

            // Act
            IntegrationPointListPage integrationPointListPage = Being.On<IntegrationPointListPage>(TestsImplementationTestFixture.Workspace.ArtifactID);
            IntegrationPointEditPage integrationPointEditPage = integrationPointListPage.NewIntegrationPoint.ClickAndGo();

            IntegrationPointViewPage integrationPointViewPage = integrationPointEditPage
                .CreateProductionToFolderIntegrationPoint(
                    integrationPointName,
                    destinationWorkspace,
                    production,
                    YesNo.Yes);

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
            integrationPointViewPage.GetOverwriteMode().ShouldBeEquivalentTo(RelativityProviderOverwrite.AppendOnly);
            integrationPointViewPage.GetExportType().ShouldBeEquivalentTo("Workspace; ImagesNatives");
            integrationPointViewPage.GetSourceDetails().ShouldBeEquivalentTo($"Production Set: {production.Name}");
            integrationPointViewPage.GetSourceWorkspaceName().ShouldBeEquivalentTo(TestsImplementationTestFixture.Workspace.Name);
            integrationPointViewPage.GetSourceRelativityInstance().ShouldBeEquivalentTo("This instance(emttest)");
            integrationPointViewPage.GetTransferredObject().ShouldBeEquivalentTo(IntegrationPointTransferredObjects.Document);
            integrationPointViewPage.GetDestinationWorkspaceName().ShouldBeEquivalentTo(destinationWorkspace.Name);
            integrationPointViewPage.GetDestinationFolderName().ShouldBeEquivalentTo(destinationWorkspace.Name);
            integrationPointViewPage.GetMultiSelectOverlayMode().ShouldBeEquivalentTo(FieldOverlayBehavior.UseFieldSettings);
            integrationPointViewPage.GetUseFolderPathInfo().ShouldBeEquivalentTo(RelativityProviderFolderPathInformation.No);
            integrationPointViewPage.GetImagePrecedence().ShouldBeEquivalentTo(RelativityProviderImagePrecedence.OriginalImages);
            integrationPointViewPage.GetCopyFilesToRepository().ShouldBeEquivalentTo(YesNo.Yes);

            #endregion

            #region 2nd column
            const int productionDocumentsCount = 5;
            integrationPointViewPage.SummaryPageGeneralTab.LogErrors.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.HasErrors.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.EmailNotificationRecipients.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.TotalOfDocuments.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.TotalOfImages.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.CreateSavedSearch.ExpectTo.BeVisible();

            integrationPointViewPage.GetLogErrors().ShouldBeEquivalentTo(YesNo.Yes);
            integrationPointViewPage.GetHasErrors().ShouldBeEquivalentTo(YesNo.No);
            integrationPointViewPage.GetEmailNotificationRecipients().Should().BeNullOrEmpty();
            integrationPointViewPage.GetTotalDocuments().ShouldBeEquivalentTo(productionDocumentsCount);
            integrationPointViewPage.GetTotalImages().ShouldBeEquivalentTo($"{productionDocumentsCount} (22.72KB)");
            integrationPointViewPage.GetCreateSavedSearch().ShouldBeEquivalentTo(YesNo.No);

            #endregion
        }

        public void EntitiesPushSummaryPage()
        {
            // Arrange
            TestsImplementationTestFixture.LoginAsStandardUser();

            string integrationPointName = nameof(EntitiesPushSummaryPage);

            Workspace destinationWorkspace = CreateDestinationWorkspace();
            const int entitiesCount = 10;
            PrepareEntities(entitiesCount).GetAwaiter().GetResult();

            // Act
            IntegrationPointListPage integrationPointListPage = Being.On<IntegrationPointListPage>(TestsImplementationTestFixture.Workspace.ArtifactID);
            IntegrationPointEditPage integrationPointEditPage = integrationPointListPage.NewIntegrationPoint.ClickAndGo();

            string viewName = "Entities - Legal Hold View";
            IntegrationPointViewPage integrationPointViewPage = integrationPointEditPage
                .CreateSyncRdoIntegrationPoint(integrationPointName, destinationWorkspace, IntegrationPointTransferredObjects.Entity, viewName);


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
            integrationPointViewPage.SummaryPageGeneralTab.MultiSelectOverlay.ExpectTo.BeVisible();

            integrationPointViewPage.GetName().ShouldBeEquivalentTo(integrationPointName);
            integrationPointViewPage.GetOverwriteMode().ShouldBeEquivalentTo(RelativityProviderOverwrite.AppendOnly);
            integrationPointViewPage.GetExportType().ShouldBeEquivalentTo("Workspace; View");
            integrationPointViewPage.GetSourceDetails().ShouldBeEquivalentTo($"View: {viewName}");
            integrationPointViewPage.GetSourceWorkspaceName().ShouldBeEquivalentTo(TestsImplementationTestFixture.Workspace.Name);
            integrationPointViewPage.GetSourceRelativityInstance().ShouldBeEquivalentTo("This instance(emttest)");
            integrationPointViewPage.GetTransferredObject().ShouldBeEquivalentTo(IntegrationPointTransferredObjects.Entity);
            integrationPointViewPage.GetDestinationWorkspaceName().ShouldBeEquivalentTo(destinationWorkspace.Name);
            integrationPointViewPage.GetMultiSelectOverlayMode().ShouldBeEquivalentTo(FieldOverlayBehavior.UseFieldSettings);

            #endregion

            #region 2nd column

            integrationPointViewPage.SummaryPageGeneralTab.LogErrors.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.HasErrors.ExpectTo.BeVisible();
            integrationPointViewPage.SummaryPageGeneralTab.EmailNotificationRecipients.ExpectTo.BeVisible();

            integrationPointViewPage.GetLogErrors().ShouldBeEquivalentTo(YesNo.Yes);
            integrationPointViewPage.GetHasErrors().ShouldBeEquivalentTo(YesNo.No);
            integrationPointViewPage.GetEmailNotificationRecipients().Should().BeNullOrEmpty();

            #endregion
        }

        private async Task PrepareEntities(int count)
        {
            using (IObjectManager objectManager = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IObjectManager>())
            {
                int entityArtifactTypeId = await GetArtifactTypeIdAsync(TestsImplementationTestFixture.Workspace.ArtifactID, "Entity").ConfigureAwait(false);

                ObjectTypeRef entityObjectType = new ObjectTypeRef()
                {
                    ArtifactTypeID = entityArtifactTypeId
                };

                FieldRef[] fields = new[]
                {
                    new FieldRef()
                    {
                        Name = "Full Name"
                    },
                    new FieldRef()
                    {
                        Name = "Email"
                    }
                };

                IReadOnlyList<IReadOnlyList<object>> values = Enumerable
                    .Range(1, count)
                    .Select(i => new List<object>()
                    {
                        $"Employee {i}",
                        $"employee-{i}@company.com"
                    })
                    .ToList();

                MassCreateResult massCreateResult = await objectManager.CreateAsync(TestsImplementationTestFixture.Workspace.ArtifactID, new MassCreateRequest()
                {
                    ObjectType = entityObjectType,
                    Fields = fields,
                    ValueLists = values
                }, CancellationToken.None).ConfigureAwait(false);

                if (!massCreateResult.Success)
                {
                    throw new Exception($"Mass creation of Entities failed: {massCreateResult.Message}");
                }
            }
        }

        private async Task<int> GetArtifactTypeIdAsync(int workspaceId, string artifactTypeName)
        {
            using (var service = RelativityFacade.Instance.GetComponent<ApiComponent>().ServiceFactory.GetServiceProxy<IObjectTypeManager>())
            {
                List<ObjectTypeIdentifier> artifactTypes = await service.GetAvailableParentObjectTypesAsync(workspaceId).ConfigureAwait(false);
                ObjectTypeIdentifier artifactType = artifactTypes.FirstOrDefault(x => x.Name == artifactTypeName);

                if (artifactType == null)
                {
                    throw new Exception($"Can't find Artifact Type: {artifactTypeName}");
                }

                return artifactType.ArtifactTypeID;
            }
        }
    }
}