using System;
using Atata;
using Relativity.IntegrationPoints.Tests.Functional.Web.Components;
using Relativity.IntegrationPoints.Tests.Functional.Web.Models;
using Relativity.Testing.Framework.Models;
using Relativity.Testing.Framework.Web.Extensions;
using Relativity.Testing.Framework.Web.Models;

namespace Relativity.IntegrationPoints.Tests.Functional.Web.Extensions
{
	internal static class IntegrationPointEditPageExtensions
	{
		public static IntegrationPointViewPage CreateSavedSearchToFolderIntegrationPoint(this IntegrationPointEditPage integrationPointEditPage,
			string integrationPointName, Workspace destinationWorkspace, KeywordSearch savedSearch,
			RelativityProviderOverwrite overwriteMode = RelativityProviderOverwrite.AppendOnly,
			YesNo copyImages = YesNo.No, RelativityProviderCopyNativeFiles copyNativesMode = RelativityProviderCopyNativeFiles.No)
		{
			var relativityProviderConnectToSourcePage = FillOutIntegrationPointEditPageForRelativityProvider(integrationPointEditPage, integrationPointName);

			var relativityProviderMapFieldsPage = FillOutRelativityProviderConnectToSourcePage(
				relativityProviderConnectToSourcePage, destinationWorkspace,
				RelativityProviderSources.SavedSearch, savedSearch.Name);

			var integrationPointViewPage = relativityProviderMapFieldsPage.MapAllFields
				.Click().ApplyModel(new RelativityProviderMapFields
				{
					Overwrite = overwriteMode,
					CopyImages = copyImages,
					CopyNativeFiles = copyNativesMode,
					PathInformation = RelativityProviderFolderPathInformation.No
				}).Save.ClickAndGo();

			return integrationPointViewPage;
		}

		public static IntegrationPointViewPage CreateProductionToFolderIntegrationPoint(
			this IntegrationPointEditPage integrationPointEditPage, string integrationPointName, 
			Workspace destinationWorkspace, Testing.Framework.Models.Production production,
			RelativityProviderOverwrite overwriteMode = RelativityProviderOverwrite.AppendOnly)
		{
			var relativityProviderConnectToSourcePage = FillOutIntegrationPointEditPageForRelativityProvider(integrationPointEditPage, integrationPointName);

			var relativityProviderMapFieldsPage = FillOutRelativityProviderConnectToSourcePage(
				relativityProviderConnectToSourcePage, destinationWorkspace, 
				RelativityProviderSources.Production, productionSetName: production.Name);

			var integrationPointViewPage = relativityProviderMapFieldsPage.ApplyModel(new
			{
				Overwrite = overwriteMode
			}).Save.ClickAndGo();

			return integrationPointViewPage;
		}

		private static RelativityProviderConnectToSourcePage FillOutIntegrationPointEditPageForRelativityProvider(IntegrationPointEditPage integrationPointEditPage, string integrationPointName)
		{
			return integrationPointEditPage.ApplyModel(new IntegrationPointEdit
			{
				Name = integrationPointName,
				Type = IntegrationPointTypes.Export,
				Destination = IntegrationPointDestinations.Relativity
			}).RelativityProviderNext.ClickAndGo();
		}

		private static RelativityProviderMapFieldsPage FillOutRelativityProviderConnectToSourcePage(RelativityProviderConnectToSourcePage relativityProviderConnectToSourcePage, Workspace destinationWorkspace, RelativityProviderSources source,
			string savedSearchName = null, string productionSetName = null)
		{
			RelativityProviderConnectToSource relativityProviderConnectToSource;
			switch (source)
			{
				case RelativityProviderSources.SavedSearch:
					relativityProviderConnectToSource = new RelativityProviderConnectToSavedSearchSource { SavedSearch = savedSearchName };
					break;
				case RelativityProviderSources.Production:
					relativityProviderConnectToSource = new RelativityProviderConnectToProductionSource { ProductionSet = productionSetName };
					break;
				default:
					throw new ArgumentException($"The provided source ({source}) for Relativity Provider is not supported.", nameof(source));
			}
			relativityProviderConnectToSource.DestinationWorkspace = $"{destinationWorkspace.Name} - {destinationWorkspace.ArtifactID}";
			relativityProviderConnectToSource.Location = RelativityProviderDestinationLocations.Folder;

			return relativityProviderConnectToSourcePage
				.ApplyModel(relativityProviderConnectToSource)
				.SelectFolder.Click().SetItem($"{destinationWorkspace.Name}")
				.Next.ClickAndGo();
		}
	}
}
