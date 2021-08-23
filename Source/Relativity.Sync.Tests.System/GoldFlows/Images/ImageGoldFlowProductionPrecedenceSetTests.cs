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
	internal sealed class ImageGoldFlowProductionPrecedenceSetTests : SystemTest
	{
		private const string HAS_IMAGES_YES_CHOICE = "5002224A-59F9-4C19-AA57-3765BDBFB676";

		private GoldFlowTestSuite _goldFlowTestSuite;
		private readonly Dataset _dataset;

		public ImageGoldFlowProductionPrecedenceSetTests()
		{
			_dataset = Dataset.TwoDocumentProduction;
		}

		protected override async Task ChildSuiteSetup()
		{
			_goldFlowTestSuite = await GoldFlowTestSuite.CreateAsync(Environment, User, ServiceFactory).ConfigureAwait(false);
		}

		[IdentifiedTest("64C9EBB4-9024-4123-BFE4-96B3C0433436")]
		[TestType.MainFlow]
		public async Task SyncJob_ShouldSyncImages_WhenImagePrecedenceIsSelected()
		{
			// Arrange
			ProductionDto production = await CreateAndImportProductionAsync(_goldFlowTestSuite.SourceWorkspace.ArtifactID, _dataset).ConfigureAwait(false);
			TridentHelper.UpdateFilePathToLocalIfNeeded(_goldFlowTestSuite.SourceWorkspace.ArtifactID, _dataset);

			GoldFlowTestSuite.IGoldFlowTestRun goldFlowTestRun = await _goldFlowTestSuite.CreateTestRunAsync((sourceWorkspace, destinationWorkspace, config) => 
				ConfigureTestRunAsync(sourceWorkspace, destinationWorkspace, config, production.ArtifactId)).ConfigureAwait(false);

			// Act
			SyncJobState result = await goldFlowTestRun.RunAsync().ConfigureAwait(false);

			// Assert
			IList<RelativityObject> documentsWithImagesInSourceWorkspace = await Rdos.QueryDocumentsAsync(ServiceFactory, _goldFlowTestSuite.SourceWorkspace.ArtifactID, $"'Production::Image Count' > 0").ConfigureAwait(false);
			IList<RelativityObject> documentsWithImagesInDestinationWorkspace = await Rdos.QueryDocumentsAsync(ServiceFactory, goldFlowTestRun.DestinationWorkspaceArtifactId, $"'Has Images' == CHOICE {HAS_IMAGES_YES_CHOICE}").ConfigureAwait(false);

			await goldFlowTestRun.AssertAsync(result, _dataset.TotalItemCount, _dataset.TotalItemCount).ConfigureAwait(false);

			documentsWithImagesInDestinationWorkspace.Count.Should().Be(_dataset.TotalDocumentCount);

			goldFlowTestRun.AssertDocuments(
				documentsWithImagesInSourceWorkspace.Select(x => x.Name).ToArray(),
				documentsWithImagesInDestinationWorkspace.Select(x => x.Name).ToArray()
			);

			await goldFlowTestRun.AssertImagesAsync(
				_goldFlowTestSuite.SourceWorkspace.ArtifactID, documentsWithImagesInSourceWorkspace.ToArray(),
				goldFlowTestRun.DestinationWorkspaceArtifactId, documentsWithImagesInDestinationWorkspace.ToArray()
			).ConfigureAwait(false);
		}

		private async Task ConfigureTestRunAsync(WorkspaceRef sourceWorkspace, WorkspaceRef destinationWorkspace, ConfigurationStub configuration, int productionId)
		{
			configuration.FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings;
			configuration.ImportOverwriteMode = ImportOverwriteMode.AppendOverlay;
			configuration.ImportImageFileCopyMode = ImportImageFileCopyMode.CopyFiles;
			configuration.ImageImport = true;
			configuration.IncludeOriginalImageIfNotFoundInProductions = false;
			configuration.ProductionImagePrecedence = new[] { productionId };

			IList<FieldMap> identifierMapping = await GetIdentifierMappingAsync(sourceWorkspace.ArtifactID, destinationWorkspace.ArtifactID).ConfigureAwait(false);
			configuration.SetFieldMappings(identifierMapping);
		}
	}
}