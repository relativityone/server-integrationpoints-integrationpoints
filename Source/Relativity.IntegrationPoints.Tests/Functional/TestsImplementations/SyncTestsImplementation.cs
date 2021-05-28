using Atata;
using System;
using System.Collections.Generic;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Web.Models;
using Relativity.Testing.Framework.Web.Navigation;
using Relativity.Testing.Framework.Web.Extensions;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using FluentAssertions;

namespace Relativity.IntegrationPoints.Tests.Functional.TestsImplementations
{
	internal class SyncTestsImplementation
	{
		private readonly ITestsImplementationTestFixture _testsImplementationTestFixture;
		private readonly Dictionary<string, Workspace> _destinationWorkspaces = new Dictionary<string, Workspace>();

		public SyncTestsImplementation(ITestsImplementationTestFixture testsImplementationTestFixture)
		{
			_testsImplementationTestFixture = testsImplementationTestFixture;
		}

		public void OnSetUpFixture()
		{
			RelativityFacade.Instance.ImportDocumentsFromCsv(_testsImplementationTestFixture.Workspace, LoadFiles.UWS_NATIVES_LOAD_FILE_PATH);

			_destinationWorkspaces.Add(nameof(SavedSearchNativesAndMetadataGoldFlow), RelativityFacade.Instance.CreateWorkspace(nameof(SavedSearchNativesAndMetadataGoldFlow)));
		}

		public void OnTearDownFixture()
		{
			foreach (var destinationWorkspace in _destinationWorkspaces)
			{
				RelativityFacade.Instance.DeleteWorkspace(destinationWorkspace.Value);
			}
		}

		public void SavedSearchNativesAndMetadataGoldFlow()
		{
			// Arrange
			string integrationPointName = nameof(SavedSearchNativesAndMetadataGoldFlow);
			Workspace destinationWorkspace = _destinationWorkspaces[nameof(SavedSearchNativesAndMetadataGoldFlow)];
			KeywordSearch keywordSearch = new KeywordSearch
			{
				Name = nameof(SavedSearchNativesAndMetadataGoldFlow),
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
							Condition = new CriteriaCondition(new NamedArtifact { Name = "Control Number" }, ConditionOperator.LessThanOrEqualTo, "AZIPPER_0011361")
						}
					}
				}
			};
			const int keywordSearchDocumentsCount = 50;
			RelativityFacade.Instance.Resolve<IKeywordSearchService>().Require(_testsImplementationTestFixture.Workspace.ArtifactID, keywordSearch);

			// Act
			var integrationPointListPage = Being.On<IntegrationPointListPage>(_testsImplementationTestFixture.Workspace.ArtifactID);
			var integrationPointEditPage = integrationPointListPage.NewIntegrationPoint.ClickAndGo();

			var relativityProviderConnectToSourcePage = integrationPointEditPage.ApplyModel(new IntegrationPointEdit
			{
				Name = integrationPointName,
				Type = IntegrationPointTypes.Export,
				Destination = IntegrationPointDestinations.Relativity
			}).RelativityProviderNext.ClickAndGo();

			var relativityProviderMapFieldsPage = relativityProviderConnectToSourcePage.ApplyModel(new RelativityProviderConnectToSource
			{
				DestinationWorkspace = $"{destinationWorkspace.Name} - {destinationWorkspace.ArtifactID}",
				Source = RelativityProviderSources.SavedSearch,
				SavedSearch = keywordSearch.Name,
				Location = RelativityProviderDestinationLocations.Folder
			}).SelectFolder.Click().SetItem($"{destinationWorkspace.Name}").Next.ClickAndGo();

			var integrationPointViewPage = relativityProviderMapFieldsPage.MapAllFields.Click().ApplyModel(new RelativityProviderMapFields
			{
				Overwrite = RelativityProviderOverwrite.AppendOnly,
				CopyImages = YesNo.No,
				CopyNativeFiles = RelativityProviderCopyNativeFiles.PhysicalFiles,
				PathInformation = RelativityProviderFolderPathInformation.No
			}).Save.ClickAndGo();

			integrationPointViewPage = integrationPointViewPage.Run.WaitTo.Within(60).BeVisible().
				Run.ClickAndGo().
				OK.ClickAndGo().
				WaitUntilJobCompleted(integrationPointName);

			// Assert
			int transferredItemsCount = Int32.Parse(integrationPointViewPage.Status.Table.Rows[y => y.Name == integrationPointName].ItemsTransferred.Content.Value);
			int workspaceDocumentCount = RelativityFacade.Instance.Resolve<IDocumentService>().GetAll(destinationWorkspace.ArtifactID).Length;

			transferredItemsCount.Should().Be(workspaceDocumentCount).And.Be(keywordSearchDocumentsCount);
		}
	}
}
