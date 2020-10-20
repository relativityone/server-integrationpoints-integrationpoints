﻿using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Relativity.Services.Workspace;
using Relativity.Sync.Storage;
using Relativity.Sync.Configuration;
using Relativity.Testing.Identification;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using NUnit.Framework;

namespace Relativity.Sync.Tests.System.GoldFlows
{
	[TestFixture]
	public class DocumentGoldFlowTests : SystemTest
	{
		private GoldFlowTestSuite _goldFlowTestSuite;
		private readonly Dataset _dataset;

		public DocumentGoldFlowTests()
		{
			_dataset = Dataset.NativesAndExtractedText;
		}

		protected override async Task ChildSuiteSetup()
		{
			_goldFlowTestSuite = await GoldFlowTestSuite.CreateAsync(Environment, User, ServiceFactory, DataTableFactory.CreateImportDataTable(_dataset, true))
				.ConfigureAwait(false);
		}

		[IdentifiedTest("25b723da-82fe-4f56-ae9f-4a8b2a4d60f4")]
		[TestType.MainFlow]
		public async Task SyncJob_Should_SyncDocuments()
		{
			// Arrange
			var goldFlowTestRun = await _goldFlowTestSuite.CreateTestRunAsync(ConfigureTestRunAsync).ConfigureAwait(false);

			// Act
			SyncJobState result = await goldFlowTestRun.RunAsync().ConfigureAwait(false);

			// Assert
			await goldFlowTestRun.AssertAsync(result, _dataset.TotalItemCount, _dataset.TotalDocumentCount).ConfigureAwait(false);
		}

		[IdentifiedTest("e4451454-ea17-4d0e-b45a-a2c43ad35add")]
		[TestType.MainFlow]
		public async Task SyncJob_Should_RetryDocuments()
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
			await goldFlowTestRun.AssertAsync(result, _dataset.TotalDocumentCount - numberOfTaggedDocuments, _dataset.TotalDocumentCount - numberOfTaggedDocuments).ConfigureAwait(false);
		}

		private async Task ConfigureTestRunAsync(WorkspaceRef sourceWorkspace, WorkspaceRef destinationWorkspace, ConfigurationStub configuration)
		{
			configuration.DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.None;
			configuration.FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings;
			configuration.FolderPathSourceFieldName = "Document Folder Path";
			configuration.ImportNativeFileCopyMode = ImportNativeFileCopyMode.DoNotImportNativeFiles;
			configuration.ImportOverwriteMode = ImportOverwriteMode.AppendOverlay;
			configuration.MoveExistingDocuments = false;

			IEnumerable<FieldMap> identifierMapping = await GetIdentifierMappingAsync(sourceWorkspace.ArtifactID, destinationWorkspace.ArtifactID).ConfigureAwait(false);
			IEnumerable<FieldMap> extractedTextMapping = await GetExtractedTextMappingAsync(sourceWorkspace.ArtifactID, destinationWorkspace.ArtifactID).ConfigureAwait(false);

			configuration.SetFieldMappings(identifierMapping.Concat(extractedTextMapping).ToList());
		}
	}
}
