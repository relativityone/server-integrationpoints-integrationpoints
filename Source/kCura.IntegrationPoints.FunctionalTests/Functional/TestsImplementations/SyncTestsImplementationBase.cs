using System;
using System.Collections.Generic;
using System.IO;
using Atata;
using Relativity.IntegrationPoints.Tests.Common.Extensions;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Web.Navigation;
using KeywordSearch = Relativity.Testing.Framework.Models.KeywordSearch;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal abstract class SyncTestsImplementationBase
    {
        protected readonly ITestsImplementationTestFixture TestsImplementationTestFixture;
        protected readonly Dictionary<string, Workspace> DestinationWorkspaces = new Dictionary<string, Workspace>();

        public SyncTestsImplementationBase(ITestsImplementationTestFixture testsImplementationTestFixture)
        {
            TestsImplementationTestFixture = testsImplementationTestFixture;
        }

        public virtual void OnSetUpFixture()
        {
            RelativityFacade.Instance.ImportDocumentsFromCsv(TestsImplementationTestFixture.Workspace, LoadFilesGenerator.GetOrCreateNativesLoadFile());
        }

        public virtual void OnTearDownFixture()
        {
            foreach (KeyValuePair<string, Workspace> destinationWorkspace in DestinationWorkspaces)
            {
                RelativityFacade.Instance.DeleteWorkspace(destinationWorkspace.Value);
            }
        }

        protected void ExecuteSavedSearchTest(KeywordSearch keywordSearch, 
            Func<Workspace, IntegrationPointEditPage, IntegrationPointViewPage> createIntegrationPointViewPageFunction, 
            Action<Workspace, IntegrationPointViewPage> assertAction)
        {
            // Arrange
            void ArrangeAction()
            {
                RelativityFacade.Instance.Resolve<IKeywordSearchService>().Require(TestsImplementationTestFixture.Workspace.ArtifactID, keywordSearch);
            }

            ExecuteTest(ArrangeAction, createIntegrationPointViewPageFunction, assertAction);
        }

        protected void ExecuteProductionTest(KeywordSearch keywordSearch,
            Func<Workspace, IntegrationPointEditPage, IntegrationPointViewPage> createIntegrationPointViewPageFunction,
            Action<Workspace, IntegrationPointViewPage> assertAction)
        {
            // Arrange
            void ArrangeAction()
            {
                RelativityFacade.Instance.Resolve<IKeywordSearchService>().Require(TestsImplementationTestFixture.Workspace.ArtifactID, keywordSearch);

                ProductionPlaceholder productionPlaceholder = new ProductionPlaceholder
                {
                    Name = keywordSearch.Name,
                    PlaceholderType = PlaceholderType.Image,
                    FileName = "Image",
                    FileData = Convert.ToBase64String(File.ReadAllBytes(DataFiles.PLACEHOLDER_IMAGE_PATH))
                };
                RelativityFacade.Instance.Resolve<IProductionPlaceholderService>().Create(TestsImplementationTestFixture.Workspace.ArtifactID, productionPlaceholder);

                
                Testing.Framework.Models.Production production = new Testing.Framework.Models.Production
                {
                    Name = keywordSearch.Name,
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
                        Name = keywordSearch.Name,
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
            }

            ExecuteTest(ArrangeAction, createIntegrationPointViewPageFunction, assertAction);
        }

        protected Workspace CreateDestinationWorkspace()
        {
            string workspaceName = $"Sync - Dest {Guid.NewGuid()}";

            Workspace workspace = RelativityFacade.Instance.CreateWorkspace(workspaceName, TestsImplementationTestFixture.Workspace.Name);

            DestinationWorkspaces.Add(workspaceName, workspace);

            workspace.InstallLegalHold();

            return workspace;
        }

        private void ExecuteTest(Action PrepareArrangeAction,
            Func<Workspace, IntegrationPointEditPage, IntegrationPointViewPage> createIntegrationPointViewPageFunction, Action<Workspace, IntegrationPointViewPage> assertAction)
        {
            // Arrange
            TestsImplementationTestFixture.LoginAsStandardUser();

            Workspace destinationWorkspace = CreateDestinationWorkspace();

            PrepareArrangeAction();

            // Act
            IntegrationPointListPage integrationPointListPage = Being.On<IntegrationPointListPage>(TestsImplementationTestFixture.Workspace.ArtifactID);
            IntegrationPointEditPage integrationPointEditPage = integrationPointListPage.NewIntegrationPoint.ClickAndGo();
            IntegrationPointViewPage integrationPointViewPage = createIntegrationPointViewPageFunction(destinationWorkspace, integrationPointEditPage);

            // Assert
            assertAction(destinationWorkspace, integrationPointViewPage);
            
        }
    }
}