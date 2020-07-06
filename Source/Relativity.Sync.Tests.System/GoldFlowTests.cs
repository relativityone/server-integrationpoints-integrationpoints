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
		private WorkspaceRef SourceWorkspace;
		private WorkspaceRef DestinationWorkspace;
		private ConfigurationStub Configuration;
		private ImportDataTableWrapper _dataSet;
		private ImportHelper _importHelper;

		private readonly Guid _documentJobHistoryMultiObjectFieldGuid = new Guid("97BC12FA-509B-4C75-8413-6889387D8EF6");


		protected override async Task ChildSuiteSetup()
		{
			SourceWorkspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);

			_importHelper = new ImportHelper(ServiceFactory);

			_dataSet = DataTableFactory.CreateImportDataTable(Dataset.NativesAndExtractedText, true);
			await _importHelper.ImportDataAsync(SourceWorkspace.ArtifactID, _dataSet).ConfigureAwait(false);
		}

		[SetUp]
		public async Task SetUp()
		{
			DestinationWorkspace = await Environment.CreateWorkspaceAsync(SourceWorkspace.Name).ConfigureAwait(false);

			Configuration = new ConfigurationStub()
			{
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.None,
				CreateSavedSearchForTags = false,
				FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings,
				FolderPathSourceFieldName = "Document Folder Path",
				ImportNativeFileCopyMode = ImportNativeFileCopyMode.DoNotImportNativeFiles,
				ImportOverwriteMode = ImportOverwriteMode.AppendOverlay,
				MoveExistingDocuments = false,
			};
			Configuration.SetEmailNotificationRecipients(string.Empty);

			Configuration.SourceWorkspaceArtifactId = SourceWorkspace.ArtifactID;
			Configuration.DestinationWorkspaceArtifactId = DestinationWorkspace.ArtifactID;
			Configuration.SavedSearchArtifactId = await Rdos.GetSavedSearchInstance(ServiceFactory, SourceWorkspace.ArtifactID).ConfigureAwait(false);
			Configuration.DataSourceArtifactId = Configuration.SavedSearchArtifactId;
			IEnumerable<FieldMap> fieldsMapping = await GetIdentifierMappingAsync(SourceWorkspace.ArtifactID, DestinationWorkspace.ArtifactID).ConfigureAwait(false);
			Configuration.SetFieldMappings(fieldsMapping.ToList());
			Configuration.JobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, SourceWorkspace.ArtifactID, $"Sync Job {DateTime.Now.ToString("yyyy MMMM dd HH.mm.ss.fff")}").ConfigureAwait(false);
			Configuration.DestinationFolderArtifactId = await Rdos.GetRootFolderInstance(ServiceFactory, DestinationWorkspace.ArtifactID).ConfigureAwait(false);

		}

		[IdentifiedTest("25b723da-82fe-4f56-ae9f-4a8b2a4d60f4")]
		public async Task SyncJob_Should_SyncDocuments()
		{
			// Arrange
			int configurationID = await Rdos.CreateSyncConfigurationRDOAsync(ServiceFactory, SourceWorkspace.ArtifactID, Configuration)
					.ConfigureAwait(false);

			var runner = new SyncRunner(new ServicesManagerStub(), AppSettings.RelativityUrl, new NullAPM(), TestLogHelper.GetLogger());

			var syncParameters = new SyncJobParameters(configurationID, SourceWorkspace.ArtifactID,
				Configuration.JobHistoryArtifactId);

			// Act
			var result = await runner.RunAsync(syncParameters, User.ArtifactID).ConfigureAwait(false);

			RelativityObject jobHistory = await Rdos
				.GetJobHistoryAsync(ServiceFactory, SourceWorkspace.ArtifactID, Configuration.JobHistoryArtifactId)
				.ConfigureAwait(false);

			// Assert
			result.Status.Should().Be(SyncJobStatus.Completed, result.Message);

			int totalItems = (int)jobHistory["Total Items"].Value;
			int itemsTranferred = (int)jobHistory["Items Transferred"].Value;

			itemsTranferred.Should().Be(totalItems);
			itemsTranferred.Should().Be(_dataSet.Data.Rows.Count);
		}

		[IdentifiedTest("e4451454-ea17-4d0e-b45a-a2c43ad35add")]
		public async Task SyncJob_Should_RetryDocuments()
		{
			// Arrange
			const int numberOfTaggedDocuemnts = 2;
			Configuration.JobHistoryToRetryId = await Rdos
				.CreateJobHistoryInstanceAsync(ServiceFactory, SourceWorkspace.ArtifactID).ConfigureAwait(false);

			int configurationID = await Rdos.CreateSyncConfigurationRDOAsync(ServiceFactory, SourceWorkspace.ArtifactID, Configuration)
				.ConfigureAwait(false);

			await TagDocumentsInSource(Configuration.JobHistoryToRetryId.Value, numberOfTaggedDocuemnts).ConfigureAwait(false);

			var runner = new SyncRunner(new ServicesManagerStub(), AppSettings.RelativityUrl, new NullAPM(), TestLogHelper.GetLogger());

			var syncParameters = new SyncJobParameters(configurationID, SourceWorkspace.ArtifactID,
				Configuration.JobHistoryArtifactId);

			// Act
			var result = await runner.RunAsync(syncParameters, User.ArtifactID).ConfigureAwait(false);

			RelativityObject jobHistory = await Rdos
				.GetJobHistoryAsync(ServiceFactory, SourceWorkspace.ArtifactID, Configuration.JobHistoryArtifactId)
				.ConfigureAwait(false);

			// Assert
			result.Status.Should().Be(SyncJobStatus.Completed, result.Message);

			int totalItems = (int)jobHistory["Total Items"].Value;
			int itemsTranferred = (int)jobHistory["Items Transferred"].Value;

			itemsTranferred.Should().Be(totalItems);
			itemsTranferred.Should().Be(_dataSet.Data.Rows.Count - numberOfTaggedDocuemnts);
		}

		private async Task TagDocumentsInSource(int jobHistoryArtifactId, int numberOfDocuments = 1)
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

			foreach (var documentArtifactId in (await GetDocumentsArtifactIds(SourceWorkspace.ArtifactID)).Take(numberOfDocuments))
			{
				var updateRequest = GetRequest(documentArtifactId);
				using (var objectManager = ServiceFactory.CreateProxy<IObjectManager>())
				{
					var result = await objectManager.UpdateAsync(SourceWorkspace.ArtifactID, updateRequest)
						.ConfigureAwait(false);
				}
			}
		}

		private async Task<IEnumerable<int>> GetDocumentsArtifactIds(int workspaceId)
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
