using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System.GoldFlows.Images
{
	internal class ImageGoldFlowLinksOnlyTests : SystemTest
	{
		private const int HAS_IMAGES_YES_CHOICE = 1034243;

		private GoldFlowTestSuite _goldFlowTestSuite;
		private readonly Dataset _dataset;

		public ImageGoldFlowLinksOnlyTests()
		{
			_dataset = Dataset.MultipleImagesPerDocument;
		}

		protected override async Task ChildSuiteSetup()
		{
			_goldFlowTestSuite = await GoldFlowTestSuite.CreateAsync(Environment, User, ServiceFactory).ConfigureAwait(false);
		}

		[IdentifiedTest("E61E099A-79D0-4271-83D9-54E29937EB46")]
		[TestType.MainFlow]
		public async Task SyncJob_ShouldPushOriginalImages_UsingLinks()
		{
			// Arrange
			await _goldFlowTestSuite.ImportDocumentsAsync(DataTableFactory.CreateImageImportDataTable(_dataset)).ConfigureAwait(false);
			GoldFlowTestSuite.IGoldFlowTestRun goldFlowTestRun = await _goldFlowTestSuite.CreateTestRunAsync(ConfigureTestRunAsync).ConfigureAwait(false);

			// Act
			SyncJobState result = await goldFlowTestRun.RunAsync().ConfigureAwait(false);

			// Assert
			result.Status.Should().Be(SyncJobStatus.Completed);

			string condition = $"'Has Images' == CHOICE {HAS_IMAGES_YES_CHOICE}";
			IList<RelativityObject> documentsWithImagesInSourceWorkspace = await Rdos.QueryDocumentsAsync(ServiceFactory, _goldFlowTestSuite.SourceWorkspace.ArtifactID, condition).ConfigureAwait(false);
			IList<RelativityObject> documentsWithImagesInDestinationWorkspace = await Rdos.QueryDocumentsAsync(ServiceFactory, goldFlowTestRun.DestinationWorkspaceArtifactId, condition).ConfigureAwait(false);

			await goldFlowTestRun.AssertAsync(result, _dataset.TotalItemCount, _dataset.TotalItemCount).ConfigureAwait(false);

			documentsWithImagesInDestinationWorkspace.Count.Should().Be(_dataset.TotalDocumentCount);

			goldFlowTestRun.AssertDocuments(
				documentsWithImagesInSourceWorkspace.Select(x => x.Name).ToArray(),
				documentsWithImagesInDestinationWorkspace.Select(x => x.Name).ToArray()
			);

			goldFlowTestRun.AssertImages(
				_goldFlowTestSuite.SourceWorkspace.ArtifactID, documentsWithImagesInSourceWorkspace.ToArray(),
				goldFlowTestRun.DestinationWorkspaceArtifactId, documentsWithImagesInDestinationWorkspace.ToArray()
			);
		}

		private async Task ConfigureTestRunAsync(WorkspaceRef sourceWorkspace, WorkspaceRef destinationWorkspace, ConfigurationStub configuration)
		{
			configuration.FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings;
			configuration.ImportOverwriteMode = ImportOverwriteMode.AppendOnly;
			configuration.ImportImageFileCopyMode = ImportImageFileCopyMode.SetFileLinks;
			configuration.ImageImport = true;

			IList<FieldMap> identifierMapping = await GetIdentifierMappingAsync(sourceWorkspace.ArtifactID, destinationWorkspace.ArtifactID).ConfigureAwait(false);
			configuration.SetFieldMappings(identifierMapping);
		}
	}
}