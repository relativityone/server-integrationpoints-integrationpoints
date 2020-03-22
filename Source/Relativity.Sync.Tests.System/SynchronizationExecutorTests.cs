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
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Workspace;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Helpers;
using Relativity.Sync.Tests.System.Stubs;
using Relativity.Testing.Identification;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.Sync]
	internal sealed class SynchronizationExecutorTests : SystemTest
	{
		private static readonly Dataset Dataset = Dataset.NativesAndExtractedText;
		private static readonly Guid JobHistoryErrorObject = new Guid("17E7912D-4F57-4890-9A37-ABC2B8A37BDB");
		private static readonly Guid ErrorMessageField = new Guid("4112B894-35B0-4E53-AB99-C9036D08269D");
		private static readonly Guid StackTraceField = new Guid("0353DBDE-9E00-4227-8A8F-4380A8891CFF");
		private static readonly Guid BatchObject = new Guid("18C766EB-EB71-49E4-983E-FFDE29B1A44E");
		private static readonly Guid TransferredItemsCountField = new Guid("B2D112CA-E81E-42C7-A6B2-C0E89F32F567");
		private static readonly Guid SyncConfigurationRelation = new Guid("F673E67F-E606-4155-8E15-CA1C83931E16");

		[IdentifiedTest("f9311c70-7094-4bed-a66e-90b1313fcd47")]
		[TestCase(1000,1)]
		[TestCase(1000,2000)]
		[TestCase(1000,3500)]
		public async Task ItShouldPassGoldFlow(int batchSize, int totalRecordsCount)
		{
			const int controlNumberFieldId = 1003667;

			string sourceWorkspaceName = $"Source.{Guid.NewGuid()}";
			string destinationWorkspaceName = $"Destination.{Guid.NewGuid()}";
			string jobHistoryName = $"JobHistory.{Guid.NewGuid()}";

			var fieldMap = new List<FieldMap>
			{
				new FieldMap
				{
					SourceField = new FieldEntry
					{
						DisplayName = "Control Number",
						FieldIdentifier = controlNumberFieldId,
						IsIdentifier = true
					},
					DestinationField = new FieldEntry()
					{
						DisplayName = "Control Number",
						FieldIdentifier = controlNumberFieldId,
						IsIdentifier = true
					}
				}
			};

			int sourceWorkspaceArtifactId = await CreateWorkspaceAsync(sourceWorkspaceName).ConfigureAwait(false);
			int destinationWorkspaceArtifactId = await CreateWorkspaceAsync(destinationWorkspaceName).ConfigureAwait(false);
			int allDocumentsSavedSearchArtifactId = await Rdos.GetSavedSearchInstance(ServiceFactory, sourceWorkspaceArtifactId).ConfigureAwait(false);

			int jobHistoryArtifactId = await Rdos.CreateJobHistoryInstance(ServiceFactory, sourceWorkspaceArtifactId, jobHistoryName).ConfigureAwait(false);
			int destinationFolderArtifactId = await Rdos.GetRootFolderInstance(ServiceFactory, destinationWorkspaceArtifactId).ConfigureAwait(false);
			int syncConfigurationArtifactId = await Rdos.CreateSyncConfigurationInstance(ServiceFactory, sourceWorkspaceArtifactId, jobHistoryArtifactId, fieldMap).ConfigureAwait(false);

			ConfigurationStub configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
				DestinationWorkspaceArtifactId = destinationWorkspaceArtifactId,
				DataSourceArtifactId = allDocumentsSavedSearchArtifactId,
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure,

				JobHistoryArtifactId = jobHistoryArtifactId,
				DestinationFolderArtifactId = destinationFolderArtifactId,
				SendEmails = false,

				TotalRecordsCount = totalRecordsCount,
				BatchSize = batchSize,
				SyncConfigurationArtifactId = syncConfigurationArtifactId,
				ImportOverwriteMode = ImportOverwriteMode.AppendOverlay,
				FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings,
				ImportNativeFileCopyMode = ImportNativeFileCopyMode.CopyFiles,
			};
			configuration.SetFieldMappings(fieldMap);

			IContainer container = ContainerHelper.Create(configuration,
				containerBuilder => containerBuilder.RegisterInstance(new ImportApiFactoryStub()).As<IImportApiFactory>()
			);

			// Initialize configuration.DestinationWorkspaceTagArtifactId
			IDestinationWorkspaceTagRepository destinationWorkspaceTagRepository = container.Resolve<IDestinationWorkspaceTagRepository>();
			DestinationWorkspaceTag destinationWorkspaceTag = await destinationWorkspaceTagRepository.CreateAsync(sourceWorkspaceArtifactId,
				destinationWorkspaceArtifactId, destinationWorkspaceName).ConfigureAwait(false);
			configuration.DestinationWorkspaceTagArtifactId = destinationWorkspaceTag.ArtifactId;

			// Import documents
			var importHelper = new ImportHelper(ServiceFactory);
			ImportDataTableWrapper dataTableWrapper = DataTableFactory.CreateImportDataTable(Dataset, extractedText: true, natives: true);
			ImportJobErrors importJobErrors = await importHelper.ImportDataAsync(sourceWorkspaceArtifactId, dataTableWrapper).ConfigureAwait(false);
			Assert.IsTrue(importJobErrors.Success, $"IAPI errors: {string.Join(global::System.Environment.NewLine, importJobErrors.Errors)}");

			if(AppSettings.IsSettingsFileSet)
			{
				#region Hopper Instance workaround explanation
				//This workaround was provided to omit Hopper Instance restrictions. IAPI which is executing on agent can't access file based on file location in database like '\\emttest\DefaultFileRepository\...'.
				//Hopper is closed for outside traffic so there is no possibility to access fileshare from Trident Agent. Jira related to this https://jira.kcura.com/browse/DEVOPS-70257.
				//If we decouple Sync from RIP and move it to RAP problem probably disappears. Right now as workaround we change on database this relative Fileshare path to local,
				//where out test data are stored. So we assume in testing that push is working correctly, but whole flow (metadata, etc.) is under tests.
				#endregion
				UpdateNativeFilePathToLocal(sourceWorkspaceArtifactId);
			}

			// Source tags creation in destination workspace
			IExecutor<IDestinationWorkspaceTagsCreationConfiguration> destinationWorkspaceTagsCreationExecutor = container.Resolve<IExecutor<IDestinationWorkspaceTagsCreationConfiguration>>();
			ExecutionResult sourceWorkspaceTagsCreationExecutorResult = await destinationWorkspaceTagsCreationExecutor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);
			Assert.AreEqual(ExecutionStatus.Completed, sourceWorkspaceTagsCreationExecutorResult.Status);

			// Data source snapshot creation
			IExecutor<IDataSourceSnapshotConfiguration> dataSourceSnapshotExecutor = container.Resolve<IExecutor<IDataSourceSnapshotConfiguration>>();
			ExecutionResult dataSourceExecutorResult = await dataSourceSnapshotExecutor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);
			Assert.AreEqual(ExecutionStatus.Completed, dataSourceExecutorResult.Status);

			// Data partitioning
			IExecutor<ISnapshotPartitionConfiguration> snapshotPartitionExecutor = container.Resolve<IExecutor<ISnapshotPartitionConfiguration>>();
			ExecutionResult snapshotPartitionExecutorResult = await snapshotPartitionExecutor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);
			Assert.AreEqual(ExecutionStatus.Completed, snapshotPartitionExecutorResult.Status);

			// ImportAPI setup
			IExecutor<ISynchronizationConfiguration> syncExecutor = container.Resolve<IExecutor<ISynchronizationConfiguration>>();

			// ACT
			ExecutionResult syncResult = await syncExecutor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			Assert.AreEqual(ExecutionStatus.Completed, syncResult.Status, await AggregateJobHistoryErrorMessagesAsync(sourceWorkspaceArtifactId, jobHistoryArtifactId, syncResult).ConfigureAwait(false));

			Assert.AreEqual(dataTableWrapper.Data.Rows.Count, await GetBatchesTransferredItemsCountAsync(sourceWorkspaceArtifactId, syncConfigurationArtifactId).ConfigureAwait(false));
		}

		private async Task<int> CreateWorkspaceAsync(string workspaceName)
		{
			WorkspaceRef workspace = await Environment
				.CreateWorkspaceWithFieldsAsync(workspaceName)
				.ConfigureAwait(false);

			return workspace.ArtifactID;
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

		private async Task<int> GetBatchesTransferredItemsCountAsync(int workspaceArtifactId, int syncConfigurationArtifactId)
		{
			int batchesTransferredItemsCount = 0;

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

					foreach(int batchArtifactId in batchesArtifactsIds)
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

						batchesTransferredItemsCount += (int) (transferredItemsCountQueryResult.Objects.Single()[TransferredItemsCountField].Value ?? default(int));
					};
				}
			}

			return batchesTransferredItemsCount;
		}

		private void UpdateNativeFilePathToLocal(int sourceWorkspaceArtifactId)
		{
			using (SqlConnection connection = CreateConnectionFromAppConfig(sourceWorkspaceArtifactId))
			{
				connection.Open();

				const string sqlStatement = @"UPDATE [File] SET Location = CONCAT(@LocalFilePath, '\NATIVES\',[Filename])";
				SqlCommand command = new SqlCommand(sqlStatement, connection);
				command.Parameters.AddWithValue("LocalFilePath", Dataset.FolderPath);

				command.ExecuteNonQuery();
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