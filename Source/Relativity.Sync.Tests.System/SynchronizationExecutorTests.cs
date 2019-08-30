using System;
using System.Collections.Generic;
using System.Linq;
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

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	internal sealed class SynchronizationExecutorTests : SystemTest
	{
		private static readonly Dataset Dataset = Dataset.NativesAndExtractedText;
		private static readonly Guid JobHistoryErrorObject = new Guid("17E7912D-4F57-4890-9A37-ABC2B8A37BDB");
		private static readonly Guid ErrorMessageField = new Guid("4112B894-35B0-4E53-AB99-C9036D08269D");
		private static readonly Guid StackTraceField = new Guid("0353DBDE-9E00-4227-8A8F-4380A8891CFF");

		[Test]
		public async Task ItShouldPassGoldFlow()
		{
			const int batchSize = 1000;
			const int controlNumberFieldId = 1003667;
			const int totalRecordsCount = 1;

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
				FieldMappings = fieldMap,

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
			Assert.AreEqual(ExecutionStatus.Completed, syncResult.Status,
				await AggregateJobHistoryErrorMessagesAsync(sourceWorkspaceArtifactId, jobHistoryArtifactId, syncResult).ConfigureAwait(false));
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
	}
}