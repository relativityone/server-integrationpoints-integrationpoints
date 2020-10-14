using System.Net;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using kCura.WinEDDS.Api;
using kCura.WinEDDS.Service;
using kCura.WinEDDS.Service.Export;
using Relativity.DataExchange;
using Relativity.Services.Workspace;
using Relativity.Sync.Storage;
using Relativity.Sync.Configuration;
using Relativity.Testing.Identification;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using NUnit.Framework;
using FluentAssertions;
using AppSettings = Relativity.Sync.Tests.System.Core.AppSettings;

namespace Relativity.Sync.Tests.System.GoldFlows
{
	[TestFixture]
	public class ImageGoldFlowTests : SystemTest
	{
		private const int _HAS_IMAGES_YES_CHOICE = 1034243;
		private const string _DOCUMENT_ARTIFACT_ID_COLUMN_NAME = "DocumentArtifactID";
		private const string _FILENAME_COLUMN_NAME = "Filename";

		private GoldFlowTestSuite _goldFlowTestSuite;

		protected override async Task ChildSuiteSetup()
		{
			_goldFlowTestSuite = await GoldFlowTestSuite.CreateAsync(Environment, User, ServiceFactory, DataTableFactory.CreateImageImportDataTable(Dataset.Images))
				.ConfigureAwait(false);

			TridentHelper.UpdateFilePathToLocalIfNeeded(_goldFlowTestSuite.SourceWorkspace.ArtifactID, Dataset.Images, false);
		}

		[IdentifiedTest("1af24688-54e2-44eb-86f4-60fb28d37df4")]
		[TestType.MainFlow]
		public async Task SyncJob_Should_SyncImages()
		{
			// Arrange
			var goldFlowTestRun = await _goldFlowTestSuite.CreateTestRunAsync(ConfigureTestRunAsync).ConfigureAwait(false);

			// Act
			SyncJobState result = await goldFlowTestRun.RunAsync().ConfigureAwait(false);

			// Assert
			IList<int> documentsWithImagesInSourceWorkspace = await Rdos.QueryDocumentIdentifiersAsync(ServiceFactory, _goldFlowTestSuite.SourceWorkspace.ArtifactID, $"'Has Images' == CHOICE {_HAS_IMAGES_YES_CHOICE}").ConfigureAwait(false);
			IList<int> documentsWithImagesInDestinationWorkspace = await Rdos.QueryDocumentIdentifiersAsync(ServiceFactory, goldFlowTestRun.DestinationWorkspaceArtifactId, $"'Has Images' == CHOICE {_HAS_IMAGES_YES_CHOICE}").ConfigureAwait(false);

			await goldFlowTestRun.AssertAsync(result, _goldFlowTestSuite.DataSetItemsCount).ConfigureAwait(false);

			documentsWithImagesInDestinationWorkspace.Count.Should().Be(_goldFlowTestSuite.DataSetItemsCount);

			AssertImages(
				_goldFlowTestSuite.SourceWorkspace.ArtifactID, documentsWithImagesInSourceWorkspace.ToArray(),
				goldFlowTestRun.DestinationWorkspaceArtifactId, documentsWithImagesInDestinationWorkspace.ToArray()
			);
		}

		[IdentifiedTest("d77b1e0c-e39d-4084-9e84-6efb40ae0fe0")]
		[TestType.MainFlow]
		public async Task SyncJob_Should_RetryImages()
		{
			// Arrange
			int jobHistoryToRetryId = -1;
			var goldFlowTestRun = await _goldFlowTestSuite.CreateTestRunAsync(async (sourceWorkspace, destinationWorkspace, configuration) =>
			{
				await ConfigureTestRunAsync(sourceWorkspace, destinationWorkspace, configuration).ConfigureAwait(false);

				jobHistoryToRetryId = await Rdos.CreateJobHistoryInstanceAsync(_goldFlowTestSuite.ServiceFactory, _goldFlowTestSuite.SourceWorkspace.ArtifactID)
					.ConfigureAwait(false);
				configuration.JobHistoryToRetryId = jobHistoryToRetryId;

			}).ConfigureAwait(false);

			const int numberOfTaggedDocuments = 2;
			await Rdos.TagDocumentsAsync(ServiceFactory, _goldFlowTestSuite.SourceWorkspace.ArtifactID, jobHistoryToRetryId, numberOfTaggedDocuments).ConfigureAwait(false);

			// Act
			SyncJobState result = await goldFlowTestRun.RunAsync().ConfigureAwait(false);

			// Assert
			await goldFlowTestRun.AssertAsync(result, _goldFlowTestSuite.DataSetItemsCount - numberOfTaggedDocuments).ConfigureAwait(false);
		}

		private async Task ConfigureTestRunAsync(WorkspaceRef sourceWorkspace, WorkspaceRef destinationWorkspace, ConfigurationStub configuration)
		{
			configuration.FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings;
			configuration.ImportOverwriteMode = ImportOverwriteMode.AppendOverlay;
			configuration.ImportImageFileCopyMode = ImportImageFileCopyMode.CopyFiles;
			configuration.ImageImport = true;
			configuration.IncludeOriginalImages = true;

			IList<FieldMap> identifierMapping = await GetIdentifierMappingAsync(sourceWorkspace.ArtifactID, destinationWorkspace.ArtifactID).ConfigureAwait(false);
			configuration.SetFieldMappings(identifierMapping);
		}

		public void AssertImages(int sourceWorkspaceId, int[] sourceWorkspaceDocumentIds, int destinationWorkspaceId, int[] destinationWorkspaceDocumentIds)
		{
			CookieContainer cookieContainer = new CookieContainer();
			IRunningContext runningContext = new RunningContext
			{
				ApplicationName = "Relativity.Sync.Tests.System.GoldFlows"
			};
			NetworkCredential credentials = LoginHelper.LoginUsernamePassword(AppSettings.RelativityUserName, AppSettings.RelativityUserPassword, cookieContainer, runningContext);
			kCura.WinEDDS.Config.ProgrammaticServiceURL = AppSettings.RelativityWebApiUrl.ToString();

			List<KeyValuePair<int, string>> sourceWorkspaceImages;
			List<string> destinationWorkspaceImages;
			using (ISearchManager searchManager = new SearchManager(credentials, cookieContainer))
			{
				sourceWorkspaceImages = searchManager.RetrieveImagesForDocuments(sourceWorkspaceId, sourceWorkspaceDocumentIds).Tables[0].AsEnumerable()
					.Select(sourceWorkspaceFile => new KeyValuePair<int, string>((int)sourceWorkspaceFile[_DOCUMENT_ARTIFACT_ID_COLUMN_NAME], (string)sourceWorkspaceFile[_FILENAME_COLUMN_NAME]))
					.ToList();

				destinationWorkspaceImages = searchManager.RetrieveImagesForDocuments(destinationWorkspaceId, destinationWorkspaceDocumentIds).Tables[0].AsEnumerable()
					.Select(sourceWorkspaceFile => (string)sourceWorkspaceFile[_FILENAME_COLUMN_NAME])
					.ToList();
			}

			foreach (KeyValuePair<int, string> sourceWorkspaceImage in sourceWorkspaceImages)
			{
				Assert.That(
					destinationWorkspaceImages.Exists(destinationWorkspaceImage => destinationWorkspaceImage == sourceWorkspaceImage.Value),
					$"Destination workspace doesn't contain the {sourceWorkspaceImage.Value} image for document {sourceWorkspaceImage.Key}.");
			}
		}
	}
}
