using System;
using System.Collections.Generic;
using System.Linq;
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
using Relativity.Testing.Identification;
using ImportJobFactory = Relativity.Sync.Tests.System.Core.Helpers.ImportJobFactory;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.Sync]
	internal class ImageSynchronizationExecutorTests : SystemTest
	{
		private const int _CONTROL_NUMBER_FIELD_ID = 1003667;
		private const string _CONTROL_NUMBER_FIELD_DISPLAY_NAME = "Control Number";

		private static readonly Dataset Dataset = Dataset.NativesAndExtractedText;
		private static readonly Guid JobHistoryErrorObject = new Guid("17E7912D-4F57-4890-9A37-ABC2B8A37BDB");
		private static readonly Guid ErrorMessageField = new Guid("4112B894-35B0-4E53-AB99-C9036D08269D");
		private static readonly Guid StackTraceField = new Guid("0353DBDE-9E00-4227-8A8F-4380A8891CFF");
		private static readonly Guid BatchObject = new Guid("18C766EB-EB71-49E4-983E-FFDE29B1A44E");
		private static readonly Guid TransferredItemsCountField = new Guid("B2D112CA-E81E-42C7-A6B2-C0E89F32F567");
		private static readonly Guid SyncConfigurationRelation = new Guid("F673E67F-E606-4155-8E15-CA1C83931E16");

		[IdentifiedTestCase("A8E4D5D7-5E70-4909-9EE0-27BA1F80E532", 1000, 1)]
		public async Task ItShouldPassGoldFlow(int batchSize, int totalRecordsCount)
		{
			string sourceWorkspaceName = $"Source.884adc74-2309-45ba-be22-9d9c8be3783d";
			string destinationWorkspaceName = $"Destination.7078b15d-3ad6-4055-8473-a77d5eed9117";

			int sourceWorkspaceArtifactId = 1021191; //await CreateWorkspaceAsync(sourceWorkspaceName).ConfigureAwait(false);
			int destinationWorkspaceArtifactId = 1021193; //await CreateWorkspaceAsync(destinationWorkspaceName).ConfigureAwait(false);

			List<FieldMap> fieldMappings = CreateControlNumberFieldMapping();

			ConfigurationStub configuration = await CreateConfigurationStubAsync(sourceWorkspaceArtifactId, destinationWorkspaceArtifactId, fieldMappings, batchSize, totalRecordsCount).ConfigureAwait(false);

			// Import documents
			//Dataset dataset = Dataset.MultipleImagesPerDocument;
			//var importHelper = new ImportHelper(ServiceFactory);
			//ImportDataTableWrapper dataTableWrapper = DataTableFactory.CreateImageImportDataTable(dataset);
			//ImportJobErrors importJobErrors = await importHelper.ImportDataAsync(sourceWorkspaceArtifactId, dataTableWrapper).ConfigureAwait(false);
			//Assert.IsTrue(importJobErrors.Success, $"IAPI errors: {string.Join(global::System.Environment.NewLine, importJobErrors.Errors)}");

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

			//Assert.AreEqual(dataTableWrapper.Data.Rows.Count, await GetBatchesTransferredItemsCountAsync(sourceWorkspaceArtifactId, configuration.SyncConfigurationArtifactId).ConfigureAwait(false));
		}

		private async Task<int> CreateWorkspaceAsync(string workspaceName)
		{
			WorkspaceRef workspace = await Environment
				.CreateWorkspaceWithFieldsAsync(workspaceName)
				.ConfigureAwait(false);

			return workspace.ArtifactID;
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
			IExecutor<IImageDataSourceSnapshotConfiguration> dataSourceSnapshotExecutor = container.Resolve<IExecutor<IImageDataSourceSnapshotConfiguration>>();

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
			IExecutor<IImageSynchronizationConfiguration> syncExecutor = container.Resolve<IExecutor<IImageSynchronizationConfiguration>>();
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
	}
}
