using System;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Tests.System.Helpers;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	internal sealed class SourceWorkspaceDataReaderTests : SystemTest
	{
		[Test]
		[Ignore("Depends on an already-created workspace plus specific pre-loaded data")]
		public async Task ItShouldWork()
		{
			const int sourceWorkspaceArtifactId = 1019631;
			const int dataSourceArtifactId = 1048428;
			const int controlNumberFieldId = 1003667;
			const int extractedTextFieldId = 1003668;
			const int totalItemsCount = 10;
			const string folderInfoFieldName = "Document Folder Path";

			string jobHistoryName = $"DataReaderTest_{Guid.NewGuid()}";
			int jobHistoryArtifactId = await Rdos.CreateJobHistoryInstance(ServiceFactory, sourceWorkspaceArtifactId, jobHistoryName).ConfigureAwait(false);
			int syncConfigurationArtifactId = await Rdos.CreateSyncConfigurationInstance(ServiceFactory, sourceWorkspaceArtifactId, jobHistoryArtifactId).ConfigureAwait(false);

			ConfigurationStub configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
				JobHistoryArtifactId = jobHistoryArtifactId,
				SyncConfigurationArtifactId = syncConfigurationArtifactId,
				DataSourceArtifactId = dataSourceArtifactId,
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.ReadFromField,
				FolderPathSourceFieldName = folderInfoFieldName,
				FieldMappings = new List<FieldMap>
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
					},
					new FieldMap
					{
						SourceField = new FieldEntry
						{
							DisplayName = "Extracted Text",
							FieldIdentifier = extractedTextFieldId
						},
						DestinationField = new FieldEntry()
						{
							DisplayName = "Extracted Text",
							FieldIdentifier = extractedTextFieldId
						}
					},
				}
			};

			var sourceServiceFactory = new ServiceFactoryStub(ServiceFactory);
			var documentFieldRepository = new DocumentFieldRepository(sourceServiceFactory, new EmptyLogger());
			var fieldManager = new FieldManager(configuration, documentFieldRepository, new List<ISpecialFieldBuilder>
			{
				new FileInfoFieldsBuilder(new NativeFileRepository(sourceServiceFactory)),
				new FolderPathFieldBuilder(new FolderPathRetriever(sourceServiceFactory, new EmptyLogger()), configuration)
			});
			var executor = new DataSourceSnapshotExecutor(sourceServiceFactory, fieldManager, new JobProgressUpdaterFactory(sourceServiceFactory, configuration), new EmptyLogger());

			ExecutionResult result = await executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			Assert.AreEqual(ExecutionStatus.Completed, result.Status);

			BatchRepository batchRepository = new BatchRepository(sourceServiceFactory);
			IBatch batch = await batchRepository.CreateAsync(sourceWorkspaceArtifactId, configuration.SyncConfigurationArtifactId, totalItemsCount, 0).ConfigureAwait(false);
			IRelativityExportBatcherFactory exportBatcherFactory = new RelativityExportBatcherFactory(sourceServiceFactory, configuration);
			IRelativityExportBatcher batcher = exportBatcherFactory.CreateRelativityExportBatcher(batch);
			SourceWorkspaceDataReader dataReader = BuildDataReader(fieldManager, configuration, batcher);
			ConsoleLogger logger = new ConsoleLogger();

			const int resultsBlockSize = 100;
			object[] tmpTable = new object[resultsBlockSize];

			while (dataReader.Read())
			{
				for (int i = 0; i < dataReader.GetValues(tmpTable); i++)
				{
					logger.LogInformation($"{dataReader.GetName(i)} [{(tmpTable[i] == null ? "null" : tmpTable[i].GetType().Name)}]: {tmpTable[i]}");
				}

				logger.LogInformation("");
			}
		}

		private static SourceWorkspaceDataReader BuildDataReader(IFieldManager fieldManager, ISynchronizationConfiguration configuration, IRelativityExportBatcher batcher)
		{
			SourceWorkspaceDataReader dataReader = new SourceWorkspaceDataReader(new BatchDataReaderBuilder(fieldManager, new ExportDataSanitizer(Enumerable.Empty<IExportFieldSanitizer>())),
				configuration,
				batcher,
				fieldManager,
				new ItemStatusMonitor(),
				new EmptyLogger());
			return dataReader;
		}
	}
}
