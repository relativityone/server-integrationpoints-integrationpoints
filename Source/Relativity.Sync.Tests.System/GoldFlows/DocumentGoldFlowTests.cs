using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using Relativity.Services.Objects;
using Relativity.Services.Workspace;
using Relativity.Services.Objects.DataContracts;
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
		
		private readonly Guid _documentJobHistoryMultiObjectFieldGuid = new Guid("97BC12FA-509B-4C75-8413-6889387D8EF6");
		
		protected override async Task ChildSuiteSetup()
		{
			_goldFlowTestSuite = await GoldFlowTestSuite.CreateAsync(Environment, User, ServiceFactory, Dataset.NativesAndExtractedText)
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
			await goldFlowTestRun.AssertAsync(result, _goldFlowTestSuite.DataSetItemsCount).ConfigureAwait(false);
		}

		[IdentifiedTest("e4451454-ea17-4d0e-b45a-a2c43ad35add")]
		[TestType.MainFlow]
		public async Task SyncJob_Should_RetryDocuments()
		{
			// Arrange
			var goldFlowTestRun = await _goldFlowTestSuite.CreateTestRunAsync(ConfigureTestRunAsync).ConfigureAwait(false);

			const int numberOfTaggedDocuments = 2;
			int jobHistoryToRetryId = await goldFlowTestRun.ConfigureForRetry().ConfigureAwait(false);
			await TagDocumentsInSourceAsync(jobHistoryToRetryId, numberOfTaggedDocuments).ConfigureAwait(false);

			// Act
			SyncJobState result = await goldFlowTestRun.RunAsync().ConfigureAwait(false);

			// Assert
			await goldFlowTestRun.AssertAsync(result, _goldFlowTestSuite.DataSetItemsCount - numberOfTaggedDocuments).ConfigureAwait(false);
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

		private async Task TagDocumentsInSourceAsync(int jobHistoryArtifactId, int numberOfDocuments = 1)
		{
			UpdateRequest GetRequest(int documentArtifactId)
			{
				return new UpdateRequest
				{
					FieldValues = new[]
					{
						new FieldRefValuePair
						{
							Field = new FieldRef {Guid = _documentJobHistoryMultiObjectFieldGuid},
							Value = new [] {new RelativityObjectRef {ArtifactID = jobHistoryArtifactId}}
						}
					},
					Object = new RelativityObjectRef
					{
						ArtifactID = documentArtifactId
					}
				};
			}

			foreach (var documentArtifactId in (await Rdos.QueryDocumentIdsAsync(ServiceFactory, _goldFlowTestSuite.SourceWorkspace.ArtifactID).ConfigureAwait(false)).Take(numberOfDocuments))
			{
				var updateRequest = GetRequest(documentArtifactId);
				using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
				{
					await objectManager.UpdateAsync(_goldFlowTestSuite.SourceWorkspace.ArtifactID, updateRequest)
						.ConfigureAwait(false);
				}
			}
		}
	}
}
