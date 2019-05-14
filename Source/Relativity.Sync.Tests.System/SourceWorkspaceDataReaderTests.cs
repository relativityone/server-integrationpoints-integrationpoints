using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Transfer;
using Relativity.Sync.Tests.System.Stubs;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	internal sealed class SourceWorkspaceDataReaderTests : SystemTest
	{
		//private int _sourceWorkspaceArtifactId;
		//private int _dataSourceArtifactId;

		protected override Task ChildSuiteSetup()
		{
			//_sourceWorkspaceArtifactId = 0;
			//_dataSourceArtifactId = 0;
			return Task.CompletedTask;
		}

		[Test]
		public async Task ItShouldWork()
		{
			const int sourceWorkspaceArtifactId = 1017928;
			const int dataSourceArtifactId = 1038052;
			var sourceServiceFactory = new SourceServiceFactoryStub(ServiceFactory);

			var executor = new DataSourceSnapshotExecutor(sourceServiceFactory, new EmptyLogger());
			const int controlNumberFieldId = 1003667;
			const int extractedTextFieldId = 1003668;
			ConfigurationStub configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
				DataSourceArtifactId = dataSourceArtifactId,
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
					},
					new FieldMap
					{
						SourceField = new FieldEntry
						{
							DisplayName = "Extracted Text",
							FieldIdentifier = extractedTextFieldId
						}
					},
				}
			};

			ExecutionResult result = await executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			Assert.AreEqual(ExecutionStatus.Completed, result.Status);

			SourceDataReaderConfiguration readerConfiguration = new SourceDataReaderConfiguration
			{
				DestinationFolderStructureBehavior = configuration.DestinationFolderStructureBehavior,
				MetadataMapping = new MetadataMapping(configuration.DestinationFolderStructureBehavior,
					configuration.FolderPathSourceFieldArtifactId, configuration.FieldMappings.ToList()),
				RunId = configuration.ExportRunId,
				SourceJobId = 0,
				SourceWorkspaceId = sourceWorkspaceArtifactId
			};

			const int totalItemCount = 229;
			const int batchSize = 30;
			IBatchRepository batchRepository = BatchRepositoryStub.Create(totalItemCount, batchSize);

			SourceWorkspaceDataReader dataReader = new SourceWorkspaceDataReader(readerConfiguration,
				new BatchDataTableBuilderFactory(new FolderPathRetriever(sourceServiceFactory, new EmptyLogger()), new NativeFileRepository(sourceServiceFactory)), 
				new RelativityExportBatcher(sourceServiceFactory, batchRepository),
				new EmptyLogger());

			int rowCount = 0;
			ConsoleLogger logger = new ConsoleLogger();
			while (dataReader.Read())
			{
				rowCount += 1;
				LogValue("Control Number", dataReader, logger);
				LogValue("Extracted Text", dataReader, logger);
				LogValue("NativeFileFilename", dataReader, logger);
				LogValue("NativeFileLocation", dataReader, logger);
				LogValue("NativeFileSize", dataReader, logger);
				LogValue("FolderPath", dataReader, logger);
				LogValue("Relativity Source Case", dataReader, logger);
				LogValue("Relativity Source Job", dataReader, logger);
				logger.LogInformation("");
			}

			logger.LogInformation($"Row count: {rowCount}");
		}

		private static void LogValue(string name, IDataReader dataReader, ISyncLog logger)
		{
			object value = dataReader[name];
			logger.LogInformation($"[{value.GetType()}] {name}: {value}");
		}
	}
}
