using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using kCura.Relativity.ImportAPI;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Helpers;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	internal sealed class SynchronizationExecutorTests : SystemTest
	{
		private SourceServiceFactoryStub _sourceServiceFactoryStub;

		protected override async Task ChildSuiteSetup()
		{
			await base.ChildSuiteSetup().ConfigureAwait(false);
			_sourceServiceFactoryStub = new SourceServiceFactoryStub(ServiceFactory);
		}

		[Test]
		public async Task ItShouldPassGoldFlow()
		{
			const int sourceWorkspaceArtifactId = 1018393;
			const int destinationWorkspaceArtifactId = 1018394;
			const string destinationWorkspaceName = "Sync 2";

			const int batchSize = 1000;
			const int allDocumentsSavedSearchArtifactId = 1038052;
			const int controlNumberFieldId = 1003667;
			const int totalRecordsCount = 1;
			string jobHistoryName = $"SysTest_{Guid.NewGuid()}";

			int jobHistoryArtifactId = await Rdos.CreateJobHistoryInstance(ServiceFactory, sourceWorkspaceArtifactId, jobHistoryName).ConfigureAwait(false);
			int destinationFolderArtifactId = await Rdos.GetRootFolderInstance(ServiceFactory, destinationWorkspaceArtifactId).ConfigureAwait(false);
			int syncConfigurationArtifactId = await Rdos.CreateSyncConfigurationInstance(ServiceFactory, sourceWorkspaceArtifactId, jobHistoryArtifactId).ConfigureAwait(false);
			
			ISyncLog logger = new EmptyLogger();
			IDateTime dateTime = new DateTimeWrapper();
			ISyncMetrics syncMetrics = new SyncMetrics(Enumerable.Empty<ISyncMetricsSink>(), new CorrelationId("SystemTests"));
			IBatchRepository batchRepository = new BatchRepository(_sourceServiceFactoryStub);
			IDestinationWorkspaceTagRepository destinationWorkspaceTagRepository = new DestinationWorkspaceTagRepository(_sourceServiceFactoryStub,
				new FederatedInstance(), new TagNameFormatter(logger), logger, syncMetrics);
			DestinationWorkspaceTag destinationWorkspaceTag = await destinationWorkspaceTagRepository.CreateAsync(sourceWorkspaceArtifactId, 
				destinationWorkspaceArtifactId, destinationWorkspaceName).ConfigureAwait(false);

			ConfigurationStub configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
				DataSourceArtifactId = allDocumentsSavedSearchArtifactId,
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure,
				FieldMappings = new List<FieldMap>
				{
					new FieldMap
					{
						SourceField = new FieldEntry
						{
							DisplayName = "Control Number",
							FieldIdentifier = controlNumberFieldId
						}
					}
				},

				JobHistoryTagArtifactId = jobHistoryArtifactId,
				DestinationFolderArtifactId = destinationFolderArtifactId,
				DestinationWorkspaceTagArtifactId = destinationWorkspaceTag.ArtifactId,
				SendEmails = false,

				TotalRecordsCount = totalRecordsCount,
				BatchSize = batchSize,
				ExportRunId = Guid.Empty,
				SyncConfigurationArtifactId = syncConfigurationArtifactId,
				ImportSettings = new ImportSettingsDto()
				{
					CaseArtifactId = destinationWorkspaceArtifactId,
					IdentityFieldId = controlNumberFieldId,

					ImportOverwriteMode = ImportOverwriteMode.AppendOverlay,
					FieldOverlayBehavior = FieldOverlayBehavior.UseFieldSettings,
				}
			};

			// Data source snapshot creation
			DataSourceSnapshotExecutor dataSourceSnapshotExecutor = new DataSourceSnapshotExecutor(_sourceServiceFactoryStub, logger);
			ExecutionResult dataSourceExecutorResult = await dataSourceSnapshotExecutor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);
			Assert.AreEqual(ExecutionStatus.Completed, dataSourceExecutorResult.Status);

			// Data partitioning
			SnapshotPartitionExecutor snapshotPartitionExecutor = new SnapshotPartitionExecutor(batchRepository, logger);
			ExecutionResult snapshotPartitionExecutorResult = await snapshotPartitionExecutor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);
			Assert.AreEqual(ExecutionStatus.Completed, snapshotPartitionExecutorResult.Status);

			// Data reader setup
			SourceWorkspaceDataReader dataReader = new SourceWorkspaceDataReader(_sourceServiceFactoryStub,
				batchRepository,
				sourceWorkspaceArtifactId,
				configuration.ExportRunId,
				batchSize,
				new MetadataMapping(configuration.DestinationFolderStructureBehavior, configuration.FolderPathSourceFieldArtifactId, configuration.FieldMappings.ToList()),
				new FolderPathRetriever(_sourceServiceFactoryStub, logger),
				new NativeFileRepository(_sourceServiceFactoryStub),
				logger);

			// ImportAPI setup
			IImportAPI importApi = new ImportAPI(AppSettings.RelativityUserName, AppSettings.RelativityUserPassword, AppSettings.RelativityWebApiUrl.AbsoluteUri);
			IImportJobFactory importJobFactory = new Executors.ImportJobFactory(
				importApi,
				dataReader,
				new BatchProgressHandlerFactory(new BatchProgressUpdater(logger), dateTime),
				new JobHistoryErrorRepository(_sourceServiceFactoryStub),
				logger);
			SynchronizationExecutor syncExecutor = new SynchronizationExecutor(batchRepository, dateTime, destinationWorkspaceTagRepository, importJobFactory, logger, syncMetrics);

			// ACT
			ExecutionResult syncResult = await syncExecutor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			Assert.AreEqual(ExecutionStatus.Completed, syncResult.Status);
		}
	}
}