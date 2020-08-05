using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Net;
using System.Security;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using kCura.Relativity.DataReaderClient;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Tests.System.Core.Stubs;
using Relativity.Sync.Tests.System.Helpers;
using Relativity.Sync.Transfer;
using Relativity.Testing.Identification;
using ImportJobFactory = Relativity.Sync.Tests.System.Core.Helpers.ImportJobFactory;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.Sync]
	internal sealed class SynchronizationExecutorTests : SystemTest
	{
		private const int _CONTROL_NUMBER_FIELD_ID = 1003667;
		private const string _CONTROL_NUMBER_FIELD_DISPLAY_NAME = "Control Number";

		private const int _USER_FIELD_ID = 1039900;
		private const string _USER_FIELD_DISPLAY_NAME = "Relativity Sync Test User";

		private const int _BATCH_COUNT_FOR_TAGGING = 3;
		
		private static readonly Dataset Dataset = Dataset.NativesAndExtractedText;
		private static readonly Guid JobHistoryErrorObject = new Guid("17E7912D-4F57-4890-9A37-ABC2B8A37BDB");
		private static readonly Guid ErrorMessageField = new Guid("4112B894-35B0-4E53-AB99-C9036D08269D");
		private static readonly Guid StackTraceField = new Guid("0353DBDE-9E00-4227-8A8F-4380A8891CFF");
		private static readonly Guid BatchObject = new Guid("18C766EB-EB71-49E4-983E-FFDE29B1A44E");
		private static readonly Guid TransferredItemsCountField = new Guid("B2D112CA-E81E-42C7-A6B2-C0E89F32F567");
		private static readonly Guid SyncConfigurationRelation = new Guid("F673E67F-E606-4155-8E15-CA1C83931E16");

		[IdentifiedTestCase("edd705b0-5d9b-42df-a0a0-a801ba0a1b0d", 1000,1)]
		[IdentifiedTestCase("3ad4d2b1-0edb-43d9-9ce2-78ab4e942c4a", 1000,2000)]
		[IdentifiedTestCase("e1fa19e6-4a27-4ba2-bb5c-c924874ccb09", 1000,3500)]
		public async Task ItShouldPassGoldFlow(int batchSize, int totalRecordsCount)
		{
			string sourceWorkspaceName = $"Source.{Guid.NewGuid()}";
			string destinationWorkspaceName = $"Destination.{Guid.NewGuid()}";

			int sourceWorkspaceArtifactId = await CreateWorkspaceAsync(sourceWorkspaceName).ConfigureAwait(false);
			int destinationWorkspaceArtifactId = await CreateWorkspaceAsync(destinationWorkspaceName).ConfigureAwait(false);

			List<FieldMap> fieldMappings = CreateControlNumberFieldMapping();

			ConfigurationStub configuration = await CreateConfigurationStubAsync(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, fieldMappings, batchSize, totalRecordsCount).ConfigureAwait(false);

			// Import documents
			var importHelper = new ImportHelper(ServiceFactory);
			ImportDataTableWrapper dataTableWrapper = DataTableFactory.CreateImportDataTable(Dataset, extractedText: true, natives: true);
			ImportJobErrors importJobErrors = await importHelper.ImportDataAsync(sourceWorkspaceArtifactId, dataTableWrapper).ConfigureAwait(false);
			Assert.IsTrue(importJobErrors.Success, $"IAPI errors: {string.Join(global::System.Environment.NewLine, importJobErrors.Errors)}");

			UpdateNativeFilePathToLocalIfNeeded(sourceWorkspaceArtifactId);

			IContainer container = CreateContainer(configuration);

			configuration.DestinationWorkspaceTagArtifactId = await GetDestinationWorkspaceTagArtifactIdAsync(container, sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, destinationWorkspaceName)
				.ConfigureAwait(false);

			Assert.AreEqual(ExecutionStatus.Completed, await CreateSourceTagsInDestinationWorkspaceAsync(container, configuration).ConfigureAwait(false));

			Assert.AreEqual(ExecutionStatus.Completed, await CreateDataSourceSnapshotAsync(container, configuration).ConfigureAwait(false));

			Assert.AreEqual(ExecutionStatus.Completed, await PartitionDataSourceSnapshotAsync(container, configuration).ConfigureAwait(false));

			// ACT
			ExecutionResult syncResult = await ExecuteSynchronizationExecutorAsync(container, configuration).ConfigureAwait(false);

			// ASSERT
			Assert.AreEqual(ExecutionStatus.Completed, syncResult.Status, await AggregateJobHistoryErrorMessagesAsync(sourceWorkspaceArtifactId, configuration.JobHistoryArtifactId, syncResult)
				.ConfigureAwait(false));

			Assert.AreEqual(dataTableWrapper.Data.Rows.Count, await GetBatchesTransferredItemsCountAsync(sourceWorkspaceArtifactId, configuration.SyncConfigurationArtifactId).ConfigureAwait(false));
		}

		[IdentifiedTest("0967e7fa-2607-48ba-bfc2-5ab6b786db86")]
		public async Task ItShouldSyncUserField()
		{
			int sourceWorkspaceArtifactId = await CreateWorkspaceAsync($"Source.{Guid.NewGuid()}").ConfigureAwait(false);

			string destinationWorkspaceName = $"Destination.{Guid.NewGuid()}";
			int destinationWorkspaceArtifactId = await CreateWorkspaceAsync(destinationWorkspaceName).ConfigureAwait(false);

			ImportJobErrors importErrors = await ImportDocumentsAsync(sourceWorkspaceArtifactId, DataTableFactory.GenerateDocumentWithUserField()).ConfigureAwait(false);
			Assert.IsTrue(importErrors.Success, $"{importErrors.Errors.Count} errors occurred during document upload: {importErrors}");

			List<FieldMap> fieldMappings = CreateControlNumberFieldMapping();
			fieldMappings.Add(new FieldMap
			{
				SourceField = new FieldEntry { DisplayName = _USER_FIELD_DISPLAY_NAME, FieldIdentifier = _USER_FIELD_ID, IsIdentifier = false },
				DestinationField = new FieldEntry { DisplayName = _USER_FIELD_DISPLAY_NAME, FieldIdentifier = _USER_FIELD_ID, IsIdentifier = false }
			});

			ConfigurationStub configuration = await CreateConfigurationStubAsync(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, fieldMappings, 1, 1).ConfigureAwait(false);

			IContainer container = CreateContainer(configuration);

			await ExecutePreSynchronizationExecutorsAsync(container, configuration, sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, destinationWorkspaceName).ConfigureAwait(false);

			// ACT
			ExecutionResult syncResult = await ExecuteSynchronizationExecutorAsync(container, configuration).ConfigureAwait(false);

			// ASSERT
			Assert.AreEqual(ExecutionStatus.Completed, syncResult.Status, await AggregateJobHistoryErrorMessagesAsync(sourceWorkspaceArtifactId, configuration.JobHistoryArtifactId, syncResult)
				.ConfigureAwait(false));
		}

		[IdentifiedTest("4150991e-5679-4e6f-afdd-a25c1ed4b9af")]
		public async Task ItShouldTagInBatches()
		{
			int sourceWorkspaceArtifactId = await CreateWorkspaceAsync($"Source.{Guid.NewGuid()}").ConfigureAwait(false);

			string destinationWorkspaceName = $"Destination.{Guid.NewGuid()}";
			int destinationWorkspaceArtifactId = await CreateWorkspaceAsync(destinationWorkspaceName).ConfigureAwait(false);

			ImportDataTableWrapper dataTableWrapper = DataTableFactory.CreateImportDataTable(Dataset, extractedText: true, natives: true);
			ImportJobErrors importErrors = await new ImportHelper(ServiceFactory).ImportDataAsync(
				sourceWorkspaceArtifactId,
				dataTableWrapper
			).ConfigureAwait(false);
			Assert.IsTrue(importErrors.Success, $"{importErrors.Errors.Count} errors occurred during document upload: {importErrors}");

			UpdateNativeFilePathToLocalIfNeeded(sourceWorkspaceArtifactId);

			List<FieldMap> fieldMappings = CreateControlNumberFieldMapping();

			ConfigurationStub configuration = await CreateConfigurationStubAsync(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, fieldMappings,
				(int)Math.Ceiling((double)dataTableWrapper.Data.Rows.Count / _BATCH_COUNT_FOR_TAGGING), dataTableWrapper.Data.Rows.Count).ConfigureAwait(false);

			IContainer container = CreateContainer(configuration);

			// Replacing DocumentTagRepository with TrackingDocumentTagRepository. I need a shower when I'm done...
			ContainerBuilder overrideContainerBuilder = new ContainerBuilder();
			container.ComponentRegistry.Registrations.Where(cr => cr.Activator.LimitType != typeof(DocumentTagRepository)).ForEach(cr => overrideContainerBuilder.RegisterComponent(cr));
			container.ComponentRegistry.Sources.ForEach(rs => overrideContainerBuilder.RegisterSource(rs));
			overrideContainerBuilder.RegisterTypes(typeof(TrackingDocumentTagRepository)).As<IDocumentTagRepository>();

			container = overrideContainerBuilder.Build();

			await ExecutePreSynchronizationExecutorsAsync(container, configuration, sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, destinationWorkspaceName).ConfigureAwait(false);

			// ACT
			ExecutionResult syncResult = await ExecuteSynchronizationExecutorAsync(container, configuration).ConfigureAwait(false);

			// ASSERT
			Assert.AreEqual(ExecutionStatus.Completed, syncResult.Status, await AggregateJobHistoryErrorMessagesAsync(sourceWorkspaceArtifactId, configuration.JobHistoryArtifactId, syncResult)
				.ConfigureAwait(false));

			IList<int> batchesTransferredItemsCounts = await GetBatchesTransferredItemsCountsAsync(sourceWorkspaceArtifactId, configuration.SyncConfigurationArtifactId).ConfigureAwait(false);
			CollectionAssert.AreEqual(batchesTransferredItemsCounts, TrackingDocumentTagRepository.TaggedDocumentsInSourceWorkspaceWithDestinationInfoCounts);
			CollectionAssert.AreEqual(batchesTransferredItemsCounts, TrackingDocumentTagRepository.TaggedDocumentsInDestinationWorkspaceWithSourceInfoCounts);
		}

		[IdentifiedTest("06cacb9a-4d8c-4cd2-8de6-2aa249925eb7")]
		[Ignore("This scenario will be valid when REL-395735 will be resolved.")]
		public async Task ItShouldCompleteWithErrors_WhenSupportedByViewerIsNull()
		{
			int sourceWorkspaceArtifactId = await CreateWorkspaceAsync($"Source.{Guid.NewGuid()}").ConfigureAwait(false);

			string destinationWorkspaceName = $"Destination.{Guid.NewGuid()}";
			int destinationWorkspaceArtifactId = await CreateWorkspaceAsync(destinationWorkspaceName).ConfigureAwait(false);

			ImportDataTableWrapper dataTableWrapper = DataTableFactory.CreateImportDataTable(Dataset, extractedText: true, natives: true);
			ImportJobErrors importErrors = await new ImportHelper(ServiceFactory).ImportDataAsync(
				sourceWorkspaceArtifactId,
				dataTableWrapper
			).ConfigureAwait(false);
			Assert.IsTrue(importErrors.Success, $"{importErrors.Errors.Count} errors occurred during document upload: {importErrors}");

			UpdateNativeFilePathToLocalIfNeeded(sourceWorkspaceArtifactId);

			List<FieldMap> fieldMappings = CreateControlNumberFieldMapping();

			ConfigurationStub configuration = await CreateConfigurationStubAsync(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, fieldMappings, 
				dataTableWrapper.Data.Rows.Count, dataTableWrapper.Data.Rows.Count).ConfigureAwait(false);

			IContainer container = CreateContainer(configuration);

			// Replacing FileInfoFieldsBuilder with NullSupportedByViewerFileInfoFieldsBuilder. Kids, don't do it in your code.
			ContainerBuilder overrideContainerBuilder = new ContainerBuilder();
			container.ComponentRegistry.Registrations.Where(cr => cr.Activator.LimitType != typeof(FileInfoFieldsBuilder)).ForEach(cr => overrideContainerBuilder.RegisterComponent(cr));
			container.ComponentRegistry.Sources.ForEach(rs => overrideContainerBuilder.RegisterSource(rs));
			overrideContainerBuilder.RegisterTypes(typeof(NullSupportedByViewerFileInfoFieldsBuilder)).As<ISpecialFieldBuilder>();

			container = overrideContainerBuilder.Build();

			await ExecutePreSynchronizationExecutorsAsync(container, configuration, sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, destinationWorkspaceName).ConfigureAwait(false);

			// ACT
			ExecutionResult syncResult = await ExecuteSynchronizationExecutorAsync(container, configuration).ConfigureAwait(false);

			// ASSERT
			Assert.AreEqual(ExecutionStatus.CompletedWithErrors, syncResult.Status, await AggregateJobHistoryErrorMessagesAsync(sourceWorkspaceArtifactId, configuration.JobHistoryArtifactId, syncResult)
				.ConfigureAwait(false));
		}

		private async Task<int> CreateWorkspaceAsync(string workspaceName)
		{
			WorkspaceRef workspace = await Environment
				.CreateWorkspaceWithFieldsAsync(workspaceName)
				.ConfigureAwait(false);

			return workspace.ArtifactID;
		}

		private async Task<ImportJobErrors> ImportDocumentsAsync(int sourceWorkspaceArtifactId, ImportDataTableWrapper documents)
		{
			ImportBulkArtifactJob documentImportJob = ImportJobFactory.CreateNonNativesDocumentImportJob(
				sourceWorkspaceArtifactId,
				await Rdos.GetRootFolderInstance(ServiceFactory, sourceWorkspaceArtifactId).ConfigureAwait(false),
				documents);

			return await ImportJobExecutor.ExecuteAsync(documentImportJob).ConfigureAwait(false);
		}

		private static List<FieldMap> CreateControlNumberFieldMapping()
		{
			return new List<FieldMap>
			{
				new FieldMap
				{
					SourceField = new FieldEntry
					{
						DisplayName = _CONTROL_NUMBER_FIELD_DISPLAY_NAME,
						FieldIdentifier = _CONTROL_NUMBER_FIELD_ID,
						IsIdentifier = true
					},
					DestinationField = new FieldEntry
					{
						DisplayName = _CONTROL_NUMBER_FIELD_DISPLAY_NAME,
						FieldIdentifier = _CONTROL_NUMBER_FIELD_ID,
						IsIdentifier = true
					}
				}
			};
		}

		private async Task<ConfigurationStub> CreateConfigurationStubAsync(
			int sourceWorkspaceArtifactId,
			int destinationWorkspaceArtifactId,
			List<FieldMap> fieldMappings,
			int batchSize,
			int totalRecordsCount)
		{
			int jobHistoryArtifactId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, sourceWorkspaceArtifactId, $"JobHistory.{Guid.NewGuid()}").ConfigureAwait(false);

			ConfigurationStub configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId,
				DataSourceArtifactId = await Rdos.GetSavedSearchInstance(ServiceFactory, sourceWorkspaceArtifactId).ConfigureAwait(false),
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure,

				JobHistoryArtifactId = jobHistoryArtifactId,
				DestinationFolderArtifactId = await Rdos.GetRootFolderInstance(ServiceFactory, destinationWorkspaceArtifactId).ConfigureAwait(false),
				SendEmails = false,

				TotalRecordsCount = totalRecordsCount,
				BatchSize = batchSize,
				SyncConfigurationArtifactId = await Rdos.CreateSyncConfigurationInstance(ServiceFactory, sourceWorkspaceArtifactId, jobHistoryArtifactId, fieldMappings).ConfigureAwait(false),
				ImportOverwriteMode = ImportOverwriteMode.AppendOverlay,
				FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings,
				ImportNativeFileCopyMode = ImportNativeFileCopyMode.CopyFiles,
			};
			configuration.SetFieldMappings(fieldMappings);

			return configuration;
		}

		private static IContainer CreateContainer(ConfigurationStub configuration)
		{
			return ContainerHelper.Create(configuration, containerBuilder =>
			{
				containerBuilder.RegisterInstance(new ImportApiFactoryStub()).As<IImportApiFactory>();
			});
		}

		private static async Task ExecutePreSynchronizationExecutorsAsync(IContainer container, ConfigurationStub configuration
			, int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, string destinationWorkspaceName)
		{
			configuration.DestinationWorkspaceTagArtifactId = await GetDestinationWorkspaceTagArtifactIdAsync(container, sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, destinationWorkspaceName)
				.ConfigureAwait(false);

			await CreateSourceTagsInDestinationWorkspaceAsync(container, configuration).ConfigureAwait(false);

			await CreateDataSourceSnapshotAsync(container, configuration).ConfigureAwait(false);

			await PartitionDataSourceSnapshotAsync(container, configuration).ConfigureAwait(false);
		}

		private static async Task<int> GetDestinationWorkspaceTagArtifactIdAsync(IContainer container, int sourceWorkspaceArtifactId, int destinationWorkspaceArtifactId, string destinationWorkspaceName)
		{
			IDestinationWorkspaceTagRepository destinationWorkspaceTagRepository = container.Resolve<IDestinationWorkspaceTagRepository>();
			DestinationWorkspaceTag destinationWorkspaceTag = await destinationWorkspaceTagRepository.CreateAsync(sourceWorkspaceArtifactId,
				destinationWorkspaceArtifactId, destinationWorkspaceName).ConfigureAwait(false);

			return destinationWorkspaceTag.ArtifactId;
		}

		private static async Task<ExecutionStatus> CreateSourceTagsInDestinationWorkspaceAsync(IContainer container, ConfigurationStub configuration)
		{
			IExecutor<IDestinationWorkspaceTagsCreationConfiguration> destinationWorkspaceTagsCreationExecutor = container.Resolve<IExecutor<IDestinationWorkspaceTagsCreationConfiguration>>();

			ExecutionResult sourceWorkspaceTagsCreationExecutorResult = await destinationWorkspaceTagsCreationExecutor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			return sourceWorkspaceTagsCreationExecutorResult.Status;
		}

		private static async Task<ExecutionStatus> CreateDataSourceSnapshotAsync(IContainer container, ConfigurationStub configuration)
		{
			IExecutor<IDataSourceSnapshotConfiguration> dataSourceSnapshotExecutor = container.Resolve<IExecutor<IDataSourceSnapshotConfiguration>>();

			ExecutionResult dataSourceExecutorResult = await dataSourceSnapshotExecutor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			return dataSourceExecutorResult.Status;
		}

		private static async Task<ExecutionStatus> PartitionDataSourceSnapshotAsync(IContainer container, ConfigurationStub configuration)
		{
			IExecutor<ISnapshotPartitionConfiguration> snapshotPartitionExecutor = container.Resolve<IExecutor<ISnapshotPartitionConfiguration>>();

			ExecutionResult snapshotPartitionExecutorResult = await snapshotPartitionExecutor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			return snapshotPartitionExecutorResult.Status;
		}

		private static Task<ExecutionResult> ExecuteSynchronizationExecutorAsync(IContainer container, ConfigurationStub configuration)
		{
			IExecutor<ISynchronizationConfiguration> syncExecutor = container.Resolve<IExecutor<ISynchronizationConfiguration>>();
			return syncExecutor.ExecuteAsync(configuration, CancellationToken.None);
		}

		private async Task<string> AggregateJobHistoryErrorMessagesAsync(int sourceWorkspaceId, int jobHistoryId, ExecutionResult syncResult)
		{
			var serviceFactoryStub = new ServiceFactoryStub(ServiceFactory);
			IEnumerable<RelativityObject> jobHistoryErrors =
				await GetAllJobErrorsAsync(serviceFactoryStub, sourceWorkspaceId, jobHistoryId).ConfigureAwait(false);

			var sb = new StringBuilder();
			sb.AppendLine($"Synchronization step failed: {syncResult.Message}: {syncResult.Exception}");
			foreach (RelativityObject err in jobHistoryErrors)
			{
				sb.AppendLine($"Item level error: {err[ErrorMessageField].Value}")
					.AppendLine((string)err[StackTraceField].Value)
					.AppendLine();
			}

			return sb.ToString();
		}

		private static async Task<IEnumerable<RelativityObject>> GetAllJobErrorsAsync(
			ISourceServiceFactoryForAdmin serviceFactory,
			int workspaceArtifactId,
			int jobHistoryArtifactId)
		{
			using (var objectManager = await serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				var request = new QueryRequest
				{
					ObjectType = new ObjectTypeRef { Guid = JobHistoryErrorObject },
					Condition = $"'Job History' == {jobHistoryArtifactId}",
					Fields = new List<FieldRef>
					{
						new FieldRef { Guid = ErrorMessageField },
						new FieldRef { Guid = StackTraceField }
					}
				};

				IEnumerable<QueryResult> results = await objectManager.QueryAllAsync(workspaceArtifactId, request).ConfigureAwait(false);

				return results.SelectMany(x => x.Objects);
			}
		}

		private async Task<IList<int>> GetBatchesTransferredItemsCountsAsync(int workspaceArtifactId, int syncConfigurationArtifactId)
		{
			List<int> batchesTransferredItemsCounts = new List<int>();

			var serviceFactory = new ServiceFactoryStub(ServiceFactory);

			using (IObjectManager objectManager = await serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false))
			{
				var batchesArtifactsIdsQueryRequest = new QueryRequest
				{
					ObjectType = new ObjectTypeRef
					{
						Guid = BatchObject
					},
					Condition = $"'{SyncConfigurationRelation}' == OBJECT {syncConfigurationArtifactId}"
				};

				QueryResultSlim batchesArtifactsIdsQueryResult = await objectManager.QuerySlimAsync(workspaceArtifactId, batchesArtifactsIdsQueryRequest, start: 1, length: int.MaxValue).ConfigureAwait(false);
				if (batchesArtifactsIdsQueryResult.TotalCount > 0)
				{
					IEnumerable<int> batchesArtifactsIds = batchesArtifactsIdsQueryResult.Objects.Select(x => x.ArtifactID);

					foreach (int batchArtifactId in batchesArtifactsIds)
					{
						QueryRequest transferredItemsCountQueryRequest = new QueryRequest
						{
							ObjectType = new ObjectTypeRef
							{
								Guid = BatchObject
							},
							Fields = new[]
							{
								new FieldRef
								{
									Guid = TransferredItemsCountField
								}
							},
							Condition = $"'ArtifactID' == {batchArtifactId}"
						};
						QueryResult transferredItemsCountQueryResult = await objectManager.QueryAsync(workspaceArtifactId, transferredItemsCountQueryRequest, start: 0, length: 1).ConfigureAwait(false);

						batchesTransferredItemsCounts.Add((int)(transferredItemsCountQueryResult.Objects.Single()[TransferredItemsCountField].Value ?? default(int)));
					};
				}
			}

			return batchesTransferredItemsCounts;
		}

		private async Task<int> GetBatchesTransferredItemsCountAsync(int workspaceArtifactId, int syncConfigurationArtifactId)
		{
			IList<int> batchesTransferredItemsCounts = await GetBatchesTransferredItemsCountsAsync(workspaceArtifactId, syncConfigurationArtifactId).ConfigureAwait(false);

			return batchesTransferredItemsCounts.Sum();
		}

		private static void UpdateNativeFilePathToLocalIfNeeded(int sourceWorkspaceArtifactId)
		{
			if (AppSettings.IsSettingsFileSet)
			{
				#region Hopper Instance workaround explanation

				//This workaround was provided to omit Hopper Instance restrictions. IAPI which is executing on agent can't access file based on file location in database like '\\emttest\DefaultFileRepository\...'.
				//Hopper is closed for outside traffic so there is no possibility to access fileshare from Trident Agent. Jira related to this https://jira.kcura.com/browse/DEVOPS-70257.
				//If we decouple Sync from RIP and move it to RAP problem probably disappears. Right now as workaround we change on database this relative Fileshare path to local,
				//where out test data are stored. So we assume in testing that push is working correctly, but whole flow (metadata, etc.) is under tests.

				#endregion
				using (SqlConnection connection = CreateConnectionFromAppConfig(sourceWorkspaceArtifactId))
				{
					connection.Open();

					const string sqlStatement =
						@"UPDATE [File] SET Location = CONCAT(@LocalFilePath, '\NATIVES\',[Filename])";
					SqlCommand command = new SqlCommand(sqlStatement, connection);
					command.Parameters.AddWithValue("LocalFilePath", Dataset.FolderPath);

					command.ExecuteNonQuery();
				}
			}
		}

		private static SqlConnection CreateConnectionFromAppConfig(int workspaceArtifactID)
		{
			SecureString password = new NetworkCredential("", AppSettings.SqlPassword).SecurePassword;
			password.MakeReadOnly();
			SqlCredential credential = new SqlCredential(AppSettings.SqlUsername, password);

			return new SqlConnection(
				GetWorkspaceConnectionString(workspaceArtifactID),
				credential);
		}

		private static string GetWorkspaceConnectionString(int workspaceArtifactID) => $"Data Source={AppSettings.SqlServer};Initial Catalog=EDDS{workspaceArtifactID}";
	}
}