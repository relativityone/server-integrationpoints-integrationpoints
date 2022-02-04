using System;
using System.Threading;
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
			string integrationPointName, Workspace destinationWorkspace, string savedSearchName,
			RelativityProviderOverwrite overwriteMode = RelativityProviderOverwrite.AppendOnly,
			YesNo copyImages = YesNo.No, RelativityProviderCopyNativeFiles copyNativesMode = RelativityProviderCopyNativeFiles.No)
		{
			RelativityProviderConnectToSourcePage relativityProviderConnectToSourcePage = FillOutIntegrationPointEditPageForRelativityProvider(integrationPointEditPage, integrationPointName);

			RelativityProviderMapFieldsPage relativityProviderMapFieldsPage = FillOutRelativityProviderConnectToSourcePage(
				relativityProviderConnectToSourcePage, destinationWorkspace,
				RelativityProviderSources.SavedSearch, savedSearchName);

			IntegrationPointViewPage integrationPointViewPage = relativityProviderMapFieldsPage.MapAllFields
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

		public static void CreateSyncRdoIntegrationPoint(this IntegrationPointEditPage page, string integrationPointName, Workspace destinationWorkspace,
			IntegrationPointTransferredObjects transferredObject, string viewName)
		{
			RelativityProviderConnectToSourcePage relativityProviderConnectToSourcePage = FillOutIntegrationPointEditPageForRelativityProvider(page, integrationPointName, transferredObject);
			FillOutRelativityProviderConnectToSourcePage(relativityProviderConnectToSourcePage, destinationWorkspace, viewName);
		}

		private static RelativityProviderConnectToSourcePage FillOutIntegrationPointEditPageForRelativityProvider(IntegrationPointEditPage integrationPointEditPage, string integrationPointName)
		{
			return integrationPointEditPage.ApplyModel(new IntegrationPointEditExport
			{
				Name = integrationPointName,
				Destination = IntegrationPointDestinations.Relativity
			}).RelativityProviderNext.ClickAndGo();
		}

		private static RelativityProviderConnectToSourcePage FillOutIntegrationPointEditPageForRelativityProvider(IntegrationPointEditPage integrationPointEditPage, string integrationPointName, IntegrationPointTransferredObjects transferredObject)
		{
			return integrationPointEditPage
				.ApplyModel(new IntegrationPointEditRdoExport
				{
					Name = integrationPointName,
					Destination = IntegrationPointDestinations.Relativity,
					TransferredObject = transferredObject
				})
				.RelativityProviderNext
				.ClickAndGo();
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
					throw new ArgumentException(@"The provided source for Relativity Provider is not supported.", nameof(source));
			}
			relativityProviderConnectToSource.DestinationWorkspace = $"{destinationWorkspace.Name} - {destinationWorkspace.ArtifactID}";
			relativityProviderConnectToSource.Location = RelativityProviderDestinationLocations.Folder;

			relativityProviderConnectToSourcePage.Source.Set(source);

			Thread.Sleep(TimeSpan.FromSeconds(2));

			return relativityProviderConnectToSourcePage
				.ApplyModel(relativityProviderConnectToSource)
				.SelectFolder.Click().SetTreeItem($"{destinationWorkspace.Name}")
				.Next.ClickAndGo();
		}

		private static void FillOutRelativityProviderConnectToSourcePage(RelativityProviderConnectToSourcePage page, Workspace destinationWorkspace, string viewName = null)
		{
			RelativityProviderConnectToViewSource model = new RelativityProviderConnectToViewSource
			{
				View = viewName,
				DestinationWorkspace = $"{destinationWorkspace.Name} - {destinationWorkspace.ArtifactID}"
			};

			page.ApplyModel(model);
		}
	}
}
