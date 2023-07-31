using System;
using System.Collections.Generic;
using System.IO;
using Atata;
using FluentAssertions;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
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
    internal class ProductionImagesSyncTestsImplementation : SyncTestsImplementationTemplate
    {
        private KeywordSearch _keywordSearch;
        private Testing.Framework.Models.Production _production;

        private readonly YesNo _copyFilesToRepository;

        public ProductionImagesSyncTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture, YesNo copyFilesToRepository) : base(testsImplementationTestFixture)
        {
            _copyFilesToRepository = copyFilesToRepository;
        }

        public override IntegrationPointViewPage CreateIntegrationPointViewPage()
        {
            _keywordSearch = RelativityFacade.Instance.Resolve<IKeywordSearchService>().Require(TestsImplementationTestFixture.Workspace.ArtifactID, new KeywordSearch
            {
                Name = nameof(ProductionImagesSyncTestsImplementation),
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

            string name = nameof(ProductionImagesSyncTestsImplementation);
            ProductionPlaceholder productionPlaceholder = new ProductionPlaceholder
            {
                Name = name,
                PlaceholderType = PlaceholderType.Image,
                FileName = "Image",
                FileData = Convert.ToBase64String(File.ReadAllBytes(DataFiles.PLACEHOLDER_IMAGE_PATH))
            };
            RelativityFacade.Instance.Resolve<IProductionPlaceholderService>().Create(TestsImplementationTestFixture.Workspace.ArtifactID, productionPlaceholder);

            _production = new Testing.Framework.Models.Production
            {
                Name = name,
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
                        Name = name,
                        ProductionType = Testing.Framework.Models.ProductionType.ImagesAndNatives,
                        SavedSearch = new NamedArtifact
                        {
                            ArtifactID = _keywordSearch.ArtifactID
                        },
                        UseImagePlaceholder = UseImagePlaceholderOption.AlwaysUseImagePlaceholder,
                        Placeholder = new NamedArtifact
                        {
                            ArtifactID = productionPlaceholder.ArtifactID
                        },
                    }
                }
            };
            RelativityFacade.Instance.ProduceProduction(TestsImplementationTestFixture.Workspace, _production);

            // Act
            IntegrationPointListPage integrationPointListPage = Being.On<IntegrationPointListPage>(TestsImplementationTestFixture.Workspace.ArtifactID);
            IntegrationPointEditPage integrationPointEditPage = integrationPointListPage.NewIntegrationPoint.ClickAndGo();

            IntegrationPointViewPage integrationPointViewPage = integrationPointEditPage
                .CreateProductionToFolderIntegrationPoint(
                    IntegrationPointName,
                    DestinationWorkspace,
                    _production,
                    _copyFilesToRepository);

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
            integrationPointViewPage.GetOverwriteMode().ShouldBeEquivalentTo(RelativityProviderOverwrite.AppendOnly);
            integrationPointViewPage.GetExportType().ShouldBeEquivalentTo("Workspace; ImagesNatives");
            integrationPointViewPage.GetSourceDetails().ShouldBeEquivalentTo($"Production Set: {_production.Name}");
            integrationPointViewPage.GetSourceWorkspaceName().ShouldBeEquivalentTo(TestsImplementationTestFixture.Workspace.Name);
            integrationPointViewPage.GetSourceRelativityInstance().ShouldBeEquivalentTo("This instance(emttest)");
            integrationPointViewPage.GetTransferredObject().ShouldBeEquivalentTo(IntegrationPointTransferredObjects.Document);
            integrationPointViewPage.GetDestinationWorkspaceName().ShouldBeEquivalentTo(DestinationWorkspace.Name);
            integrationPointViewPage.GetDestinationFolderName().ShouldBeEquivalentTo(DestinationWorkspace.Name);
            integrationPointViewPage.GetMultiSelectOverlayMode().ShouldBeEquivalentTo(FieldOverlayBehavior.UseFieldSettings);
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

            // Assert
            int transferredItemsCount = integrationPointViewPage.GetTransferredItemsCount(IntegrationPointName);
            int workspaceDocumentCount = RelativityFacade.Instance.Resolve<IDocumentService>().GetAll(DestinationWorkspace.ArtifactID).Length;

            const int productionDocumentsCount = 5;
            transferredItemsCount.Should().Be(workspaceDocumentCount).And.Be(productionDocumentsCount);

            GetCorrectlyTaggedDocumentsCount(sourceDocs, "Relativity Destination Case", expectedDestinationCaseTag).Should().Be(transferredItemsCount);
            GetCorrectlyTaggedDocumentsCount(destinationDocs, "Relativity Source Case", expectedSourceCaseTag).Should().Be(transferredItemsCount);
            GetCorrectlyTaggedDocumentsCount(destinationDocs, "Relativity Source Job", expectedSourceJobTag).Should().Be(transferredItemsCount);

            BillingFlagAssertion.AssertFiles(DestinationWorkspace.ArtifactID, expectBillable: _copyFilesToRepository == YesNo.Yes);
        }
    }
}
