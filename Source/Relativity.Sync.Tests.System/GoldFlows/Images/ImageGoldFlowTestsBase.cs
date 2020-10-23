using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.WinEDDS.Api;
using kCura.WinEDDS.Service;
using kCura.WinEDDS.Service.Export;
using NUnit.Framework;
using Relativity.DataExchange;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Helpers;
using Relativity.Testing.Identification;
using AppSettings = Relativity.Sync.Tests.System.Core.AppSettings;

namespace Relativity.Sync.Tests.System.GoldFlows.Images
{
	[TestFixture]
	public abstract class ImageGoldFlowTestsBase : SystemTest
	{
		public int ExpectedItemsForRetry { get; }
		public int ExpectedDocumentsForRetry { get; }
		private readonly Dataset _dataset;
		private const int _HAS_IMAGES_YES_CHOICE = 1034243;

		private GoldFlowTestSuite _goldFlowTestSuite;

		internal ImageGoldFlowTestsBase(Dataset dataset, int expectedItemsForRetry, int expectedDocumentsForRetry)
		{
			ExpectedItemsForRetry = expectedItemsForRetry;
			ExpectedDocumentsForRetry = expectedDocumentsForRetry;
			_dataset = dataset;
		}

		protected override async Task ChildSuiteSetup()
		{
			_goldFlowTestSuite = await GoldFlowTestSuite.CreateAsync(Environment, User, ServiceFactory, DataTableFactory.CreateImageImportDataTable(_dataset))
				.ConfigureAwait(false);

			TridentHelper.UpdateFilePathToLocalIfNeeded(_goldFlowTestSuite.SourceWorkspace.ArtifactID, _dataset, false);
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
			IList<RelativityObject> documentsWithImagesInSourceWorkspace = await Rdos.QueryDocumentsAsync(ServiceFactory, _goldFlowTestSuite.SourceWorkspace.ArtifactID, $"'Has Images' == CHOICE {_HAS_IMAGES_YES_CHOICE}").ConfigureAwait(false);
			IList<RelativityObject> documentsWithImagesInDestinationWorkspace = await Rdos.QueryDocumentsAsync(ServiceFactory, goldFlowTestRun.DestinationWorkspaceArtifactId, $"'Has Images' == CHOICE {_HAS_IMAGES_YES_CHOICE}").ConfigureAwait(false);

			await goldFlowTestRun.AssertAsync(result, _dataset.TotalItemCount, _dataset.TotalDocumentCount).ConfigureAwait(false);

			documentsWithImagesInDestinationWorkspace.Count.Should().Be(_dataset.TotalDocumentCount);

			AssertDocuments(
				documentsWithImagesInSourceWorkspace.Select(x => x.Name).ToArray(),
				documentsWithImagesInDestinationWorkspace.Select(x => x.Name).ToArray()
				);

			AssertImages(
				_goldFlowTestSuite.SourceWorkspace.ArtifactID, documentsWithImagesInSourceWorkspace.ToArray(),
				goldFlowTestRun.DestinationWorkspaceArtifactId, documentsWithImagesInDestinationWorkspace.ToArray()
			);

			await goldFlowTestRun.AssertAsync(result, _dataset.TotalItemCount, documentsWithImagesInDestinationWorkspace.Count).ConfigureAwait(false);
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

			const int numberOfTaggedDocuments = 1;
			await Rdos.TagDocumentsAsync(ServiceFactory, _goldFlowTestSuite.SourceWorkspace.ArtifactID, jobHistoryToRetryId, numberOfTaggedDocuments).ConfigureAwait(false);

			// Act
			SyncJobState result = await goldFlowTestRun.RunAsync().ConfigureAwait(false);


			// Assert
			IList<RelativityObject> documentsWithImagesInSourceWorkspace = await Rdos.QueryDocumentsAsync(ServiceFactory, _goldFlowTestSuite.SourceWorkspace.ArtifactID, $"('Has Images' == CHOICE {_HAS_IMAGES_YES_CHOICE}) AND (NOT 'Job History' SUBQUERY ('Job History' INTERSECTS MULTIOBJECT [{jobHistoryToRetryId}]))").ConfigureAwait(false);
			IList<RelativityObject> documentsWithImagesInDestinationWorkspace = await Rdos.QueryDocumentsAsync(ServiceFactory, goldFlowTestRun.DestinationWorkspaceArtifactId, $"'Has Images' == CHOICE {_HAS_IMAGES_YES_CHOICE}").ConfigureAwait(false);

			AssertDocuments(
				documentsWithImagesInSourceWorkspace.Select(x => x.Name).ToArray(),
				documentsWithImagesInDestinationWorkspace.Select(x => x.Name).ToArray()
			);

			await goldFlowTestRun.AssertAsync(result, ExpectedItemsForRetry, ExpectedDocumentsForRetry).ConfigureAwait(false);
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

		public void AssertImages(int sourceWorkspaceId, RelativityObject[] sourceWorkspaceDocuments, int destinationWorkspaceId, RelativityObject[] destinationWorkspaceDocumentIds)
		{
			string GetExpectedIdentifier(string controlNumber, int index)
			{
				if (index == 0)
				{
					return controlNumber;
				}

				return $"{controlNumber}_{index}";
			}

			CookieContainer cookieContainer = new CookieContainer();
			IRunningContext runningContext = new RunningContext
			{
				ApplicationName = "Relativity.Sync.Tests.System.GoldFlows"
			};
			NetworkCredential credentials = LoginHelper.LoginUsernamePassword(AppSettings.RelativityUserName, AppSettings.RelativityUserPassword, cookieContainer, runningContext);
			kCura.WinEDDS.Config.ProgrammaticServiceURL = AppSettings.RelativityWebApiUrl.ToString();

			ILookup<int, TestImageFile> sourceWorkspaceImages;
			Dictionary<string, TestImageFile> destinationWorkspaceImages;

			Dictionary<int, string> sourceWorkspaceDocumentNames = sourceWorkspaceDocuments.ToDictionary(x => x.ArtifactID, x => x.Name);

			using (ISearchManager searchManager = new SearchManager(credentials, cookieContainer))
			{
				var dataTable = searchManager.RetrieveImagesForDocuments(sourceWorkspaceId, sourceWorkspaceDocuments.Select(x => x.ArtifactID).ToArray()).Tables[0];
				sourceWorkspaceImages = dataTable.AsEnumerable()
					.Select(TestImageFile.GetImageFile)
					.ToLookup(x => x.DocumentArtifactId, x => x);


				destinationWorkspaceImages = searchManager.RetrieveImagesForDocuments(destinationWorkspaceId, destinationWorkspaceDocumentIds.Select(x => x.ArtifactID).ToArray()).Tables[0].AsEnumerable()
					.Select(TestImageFile.GetImageFile)
					.ToDictionary(x => x.Identifier, x => x);
			}


			foreach (var sourceDocumentImages in sourceWorkspaceImages)
			{
				int i = 0;
				foreach (var imageFile in sourceDocumentImages)
				{
					var expectedIdentifier =
						GetExpectedIdentifier(sourceWorkspaceDocumentNames[imageFile.DocumentArtifactId], i);

					destinationWorkspaceImages.ContainsKey(expectedIdentifier).Should()
						.BeTrue($"Image [{sourceDocumentImages.Key} => {expectedIdentifier}] was not pushed to destination workspace");

					var destinationImage = destinationWorkspaceImages[expectedIdentifier];

					TestImageFile.AssertAreEquivalent(imageFile, destinationImage, expectedIdentifier);
					i++;
				}
			}
		}
	}
}
