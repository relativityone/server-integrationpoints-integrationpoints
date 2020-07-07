using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Core.Runner;
using Relativity.Sync.Tests.System.Core.Stubs;
using Relativity.Telemetry.APM;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	public class GoldFlowTests : SystemTest
	{
		private WorkspaceRef _sourceWorkspace;
		private WorkspaceRef _destinationWorkspace;
		private ConfigurationStub _configuration;
		private ImportDataTableWrapper _dataSet;
		private ImportHelper _importHelper;

		private readonly Guid _documentJobHistoryMultiObjectFieldGuid = new Guid("97BC12FA-509B-4C75-8413-6889387D8EF6");


		protected override async Task ChildSuiteSetup()
		{
			_sourceWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);

			_importHelper = new ImportHelper(ServiceFactory);

			_dataSet = DataTableFactory.CreateImportDataTable(Dataset.NativesAndExtractedText, true);
			await _importHelper.ImportDataAsync(_sourceWorkspace.ArtifactID, _dataSet).ConfigureAwait(false);
		}

		[SetUp]
		public async Task SetUp()
		{
			_destinationWorkspace = await Environment.CreateWorkspaceAsync(_sourceWorkspace.Name).ConfigureAwait(false);

			_configuration = new ConfigurationStub()
			{
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.None,
				CreateSavedSearchForTags = false,
				FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings,
				FolderPathSourceFieldName = "Document Folder Path",
				ImportNativeFileCopyMode = ImportNativeFileCopyMode.DoNotImportNativeFiles,
				ImportOverwriteMode = ImportOverwriteMode.AppendOverlay,
				MoveExistingDocuments = false,
			};
			_configuration.SetEmailNotificationRecipients(string.Empty);

			_configuration.SourceWorkspaceArtifactId = _sourceWorkspace.ArtifactID;
			_configuration.DestinationWorkspaceArtifactId = _destinationWorkspace.ArtifactID;
			_configuration.SavedSearchArtifactId = await Rdos.GetSavedSearchInstance(ServiceFactory, _sourceWorkspace.ArtifactID).ConfigureAwait(false);
			_configuration.DataSourceArtifactId = _configuration.SavedSearchArtifactId;

			IEnumerable<FieldMap> identifierMapping = await GetIdentifierMappingAsync(_sourceWorkspace.ArtifactID, _destinationWorkspace.ArtifactID).ConfigureAwait(false);
			IEnumerable<FieldMap> extractedTextMapping = await GetExtractedTextMappingAsync(_sourceWorkspace.ArtifactID, _destinationWorkspace.ArtifactID).ConfigureAwait(false);

			_configuration.SetFieldMappings(identifierMapping.Concat(extractedTextMapping).ToList());

			_configuration.JobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, _sourceWorkspace.ArtifactID, $"Sync Job {DateTime.Now.ToString("yyyy MMMM dd HH.mm.ss.fff")}").ConfigureAwait(false);
			_configuration.DestinationFolderArtifactId = await Rdos.GetRootFolderInstance(ServiceFactory, _destinationWorkspace.ArtifactID).ConfigureAwait(false);
		}

		[IdentifiedTest("25b723da-82fe-4f56-ae9f-4a8b2a4d60f4")]
		[TestType.MainFlow]
		public async Task SyncJob_Should_SyncDocuments()
		{
			// Arrange
			int configurationID = await Rdos.CreateSyncConfigurationRDOAsync(ServiceFactory, _sourceWorkspace.ArtifactID, _configuration)
					.ConfigureAwait(false);

			var runner = new SyncRunner(new ServicesManagerStub(), AppSettings.RelativityUrl, new NullAPM(), TestLogHelper.GetLogger());

			var syncParameters = new SyncJobParameters(configurationID, _sourceWorkspace.ArtifactID,
				_configuration.JobHistoryArtifactId);

			// Act
			var result = await runner.RunAsync(syncParameters, User.ArtifactID).ConfigureAwait(false);

			RelativityObject jobHistory = await Rdos
				.GetJobHistoryAsync(ServiceFactory, _sourceWorkspace.ArtifactID, _configuration.JobHistoryArtifactId)
				.ConfigureAwait(false);

			// Assert
			result.Status.Should().Be(SyncJobStatus.Completed, result.Message);

			int totalItems = (int)jobHistory["Total Items"].Value;
			int itemsTranferred = (int)jobHistory["Items Transferred"].Value;

			itemsTranferred.Should().Be(totalItems);
			itemsTranferred.Should().Be(_dataSet.Data.Rows.Count);
		}

		[IdentifiedTest("e4451454-ea17-4d0e-b45a-a2c43ad35add")]
		[TestType.MainFlow]
		public async Task SyncJob_Should_RetryDocuments()
		{
			// Arrange
			const int numberOfTaggedDocuments = 2;
			_configuration.JobHistoryToRetryId = await Rdos
				.CreateJobHistoryInstanceAsync(ServiceFactory, _sourceWorkspace.ArtifactID).ConfigureAwait(false);

			int configurationID = await Rdos.CreateSyncConfigurationRDOAsync(ServiceFactory, _sourceWorkspace.ArtifactID, _configuration)
				.ConfigureAwait(false);

			await TagDocumentsInSourceAsync(_configuration.JobHistoryToRetryId.Value, numberOfTaggedDocuments).ConfigureAwait(false);

			var runner = new SyncRunner(new ServicesManagerStub(), AppSettings.RelativityUrl, new NullAPM(), TestLogHelper.GetLogger());

			var syncParameters = new SyncJobParameters(configurationID, _sourceWorkspace.ArtifactID,
				_configuration.JobHistoryArtifactId);

			// Act
			var result = await runner.RunAsync(syncParameters, User.ArtifactID).ConfigureAwait(false);

			RelativityObject jobHistory = await Rdos
				.GetJobHistoryAsync(ServiceFactory, _sourceWorkspace.ArtifactID, _configuration.JobHistoryArtifactId)
				.ConfigureAwait(false);

			// Assert
			result.Status.Should().Be(SyncJobStatus.Completed, result.Message);

			int totalItems = (int)jobHistory["Total Items"].Value;
			int itemsTranferred = (int)jobHistory["Items Transferred"].Value;

			itemsTranferred.Should().Be(totalItems);
			itemsTranferred.Should().Be(_dataSet.Data.Rows.Count - numberOfTaggedDocuments);
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

			foreach (var documentArtifactId in (await GetDocumentsArtifactIdsAsync(_sourceWorkspace.ArtifactID).ConfigureAwait(false)).Take(numberOfDocuments))
			{
				var updateRequest = GetRequest(documentArtifactId);
				using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
				{
					await objectManager.UpdateAsync(_sourceWorkspace.ArtifactID, updateRequest)
						.ConfigureAwait(false);
				}
			}
		}

		private async Task<IEnumerable<int>> GetDocumentsArtifactIdsAsync(int workspaceId)
		{
			var query = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Document }
			};

			using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
			{
				var result = await objectManager
					.QuerySlimAsync(workspaceId, query, 0, _dataSet.Data.Rows.Count)
					.ConfigureAwait(false);

				return result.Objects.Select(x => x.ArtifactID).ToList();
			}
		}
	}
}
