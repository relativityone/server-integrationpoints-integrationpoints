using System.Collections.Generic;
using Atata;
using FluentAssertions;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Helpers.LoadFiles;
using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models.ExportToLoadFileOutputSettings;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Web.Extensions;
using Relativity.Testing.Framework.Web.Navigation;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
    internal class ExportToLoadFileTestImplementation
    {
        private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;
        private static readonly string _savedSearch = nameof(ExportToLoadFilesNativesGoldFlow);
        private static readonly int _startExportAtRecord = 1;
        private int _workspaceArtifactId;
        private int _keywordSearchDocumentsCount = 5;


        public ExportToLoadFileTestImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
        {
            _testsImplementationTestFixture = testsImplementationTestFixture;
        }

        public void OnSetUpFixture()
        {
            RelativityFacade.Instance.ImportDocumentsFromCsv(_testsImplementationTestFixture.Workspace, LoadFilesGenerator.GetOrCreateNativesLoadFile());
        }

        public void ExportToLoadFilesNativesGoldFlow()
        {
            _testsImplementationTestFixture.LoginAsStandardUser();
            _workspaceArtifactId = _testsImplementationTestFixture.Workspace.ArtifactID;
            string integrationPointName = nameof(ExportToLoadFileTestImplementation);

            KeywordSearch keywordSearch = new KeywordSearch
            {
                Name = _savedSearch,
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
            RelativityFacade.Instance.Resolve<IKeywordSearchService>().Require(_workspaceArtifactId, keywordSearch);

            // Act 
            var integrationPointListPage = Being.On<IntegrationPointListPage>(_workspaceArtifactId);
            var integrationPointEditPage = integrationPointListPage.NewIntegrationPoint.ClickAndGo();

            var exportToLoadFileConnectToSourcePage = FillOutIntegrationPointEditPageForExportToLoadFileTest(integrationPointEditPage, integrationPointName);

            var exportToLoadFileDestinationInformation = FillOutIntegrationPointConnectToSourcePageForExportToLoadFileTest(exportToLoadFileConnectToSourcePage);

            var integrationPointViewPage = FillOutIntegrationPointEditPageForExportToLoadFileDestinationDetails(exportToLoadFileDestinationInformation, _workspaceArtifactId);

            integrationPointViewPage = integrationPointViewPage.RunIntegrationPoint(integrationPointName);

            // Assert
            int transferredItemsCount = integrationPointViewPage.GetTransferredItemsCount(integrationPointName);
            transferredItemsCount.Should().Be(_keywordSearchDocumentsCount);
        }

        private static ExportToLoadFileConnectToSourcePage FillOutIntegrationPointEditPageForExportToLoadFileTest(
            IntegrationPointEditPage integrationPointEditPage, string integrationPointName)
        {
            IntegrationPointEdit pageModel = new IntegrationPointEditExport
            {
                Name = integrationPointName,
                Destination = IntegrationPointDestinations.LoadFile
            };

            return integrationPointEditPage.ApplyModel(pageModel).ExportToLoadFileNext.ClickAndGo();
        }
        
        private static ExportToLoadFileDestinationInformationPage FillOutIntegrationPointConnectToSourcePageForExportToLoadFileTest(
            ExportToLoadFileConnectToSourcePage webPage )
        {
            ExportToLoadFileConnectToSavedSearchSource pageModel = new ExportToLoadFileConnectToSavedSearchSource()
            {
                StartExportAtRecord = _startExportAtRecord,
                SavedSearch = _savedSearch
            };
            
            return webPage.ApplyModel(pageModel).Next.ClickAndGo();
        }

        private static IntegrationPointViewPage FillOutIntegrationPointEditPageForExportToLoadFileDestinationDetails(ExportToLoadFileDestinationInformationPage webPage, int WorkspaceArtifactID)
        {
            webPage.LoadFile.Should.BeDisabled();
            webPage.LoadFile.Should.BeChecked();
            webPage.Natives.Check();
            webPage.Images.Check();

            webPage.SetDestinationFolder(WorkspaceArtifactID);
            webPage.ApplyModel(new ExportToLoadFileOutputSettingsModel());

            webPage.IncludeNativeFilesPath.Should.BeChecked();
            webPage.OverwriteFiles.Check();
            webPage.ExportMultipleChoiceFieldsAsNested.Check();
            webPage.AppendOriginalFileName.Check();

            return webPage.Save.Wait(Until.Visible)
                .Save.ClickAndGo();
        }
    }
}
