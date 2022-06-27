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
    internal class ProductionImagesSyncTestsImplemention : SyncTestsImplementationTemplate
    {
        private KeywordSearch _keywordSearch;
        private Testing.Framework.Models.Production _production;
        private readonly YesNo _copyFilesToRepository;
        

        public ProductionImagesSyncTestsImplemention(ITestsImplementationTestFixture testsImplementationTestFixture, YesNo copyFilesToRepository) : base(testsImplementationTestFixture)
        {
            _copyFilesToRepository = copyFilesToRepository;
        }

        public override IntegrationPointViewPage CreateIntegrationPointViewPage(Workspace workspace)
        {
            TestsImplementationTestFixture.LoginAsStandardUser();

            IntegrationPointName = $"{nameof(ProductionImagesSyncTestsImplemention)} - {Guid.NewGuid()}";

            DestinationWorkspace = CreateDestinationWorkspace();

            _keywordSearch = RelativityFacade.Instance.Resolve<IKeywordSearchService>().Require(TestsImplementationTestFixture.Workspace.ArtifactID, new KeywordSearch
            {
                Name = nameof(ProductionImagesSyncTestsImplemention),
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

            string name = nameof(ProductionImagesSyncTestsImplemention);
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
                    YesNo.Yes);

            return integrationPointViewPage;
        }

        public override void RunIntegrationPoint(IntegrationPointViewPage integrationPointViewPage)
        {
            integrationPointViewPage = integrationPointViewPage.RunIntegrationPoint(IntegrationPointName);
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
            integrationPointViewPage.GetTotalImages().ShouldBeEquivalentTo($"{productionDocumentsCount} (22.72 KB)");
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
