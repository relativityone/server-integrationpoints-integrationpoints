using Atata;
using System;
using System.Collections.Generic;
using Relativity.Testing.Framework;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Api.Services;
using Relativity.Testing.Framework.Web.Models;
using Relativity.Testing.Framework.Web.Navigation;
using Relativity.Testing.Identification;
using Relativity.IntegrationPoints.Tests.Functional.Helpers;
using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using Relativity.Testing.Framework.Web.Extensions;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using FluentAssertions;

namespace Relativity.IntegrationPoints.Tests.Functional.CI
{
	[IdentifiedTestFixture("74f17f40-697f-42b7-bad5-f83b8eeaa86a", Description = "RIP SYNC GOLD FLOWS")]
	[TestType.UI, TestType.MainFlow]
	public class SyncTests : TestsBase
	{
		private readonly  Dictionary<string, Workspace> _destinationWorkspaces = new Dictionary<string, Workspace>();
		public SyncTests()
			: base(nameof(SyncTests))
		{ }

		protected override void OnSetUpFixture()
		{
			base.OnSetUpFixture();

			RelativityFacade.Instance.ImportDocumentsFromCsv(_workspace, LoadFiles.UWS_NATIVES_LOAD_FILE_PATH);

			_destinationWorkspaces.Add(nameof(SavedSearch_NativesAndMetadata_GoldFlow), RelativityFacade.Instance.CreateWorkspace(nameof(SavedSearch_NativesAndMetadata_GoldFlow)));
		}

		protected override void OnTearDownFixture()
		{
			base.OnTearDownFixture();

			foreach (var destinationWorkspace in _destinationWorkspaces)
			{
				RelativityFacade.Instance.DeleteWorkspace(destinationWorkspace.Value);
			}
		}

		[IdentifiedTest("b0afe8eb-e898-4763-9f95-e998f220b421")]
		public void SavedSearch_NativesAndMetadata_GoldFlow()
		{
			// Arrange
			string integrationPointName = Testing.Framework.Randomizer.GetString($"{nameof(SavedSearch_NativesAndMetadata_GoldFlow)} {{0}}");
			Workspace destinationWorkspace = _destinationWorkspaces[nameof(SavedSearch_NativesAndMetadata_GoldFlow)];
			KeywordSearch keywordSearch = new KeywordSearch
			{
				Name = nameof(SavedSearch_NativesAndMetadata_GoldFlow),
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
			RelativityFacade.Instance.Resolve<IKeywordSearchService>().Require(_workspace.ArtifactID, keywordSearch);

			// Act
			var integrationPointListPage = Being.On<IntegrationPointListPage>(_workspace.ArtifactID);
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

			transferredItemsCount.Should().Be(workspaceDocumentCount).And.Be(2);
		}
	}
}
