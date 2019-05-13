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
using Relativity.Sync.Transfer;

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

			SourceWorkspaceDataReaderConfiguration readerConfiguration = new SourceWorkspaceDataReaderConfiguration
			{
				DestinationFolderStructureBehavior = configuration.DestinationFolderStructureBehavior,
				MetadataMapping = new MetadataMapping(configuration.DestinationFolderStructureBehavior,
					configuration.FolderPathSourceFieldArtifactId, configuration.FieldMappings.ToList()),
				RunId = configuration.ExportRunId,
				SourceJobId = 0,
				SourceWorkspaceId = sourceWorkspaceArtifactId
			};

			const int resultsBlockSize = 100;
			SourceWorkspaceDataReader dataReader = new SourceWorkspaceDataReader(readerConfiguration,
				sourceServiceFactory,
				new BatchRepository(sourceServiceFactory), 
				new FolderPathRetriever(sourceServiceFactory, new EmptyLogger()),
				new NativeFileRepository(sourceServiceFactory),
				new EmptyLogger(),
				resultsBlockSize);

			ConsoleLogger logger = new ConsoleLogger();
			while (dataReader.Read())
			{
				string controlNumber = (string) dataReader["Control Number"];
				logger.LogInformation($"Control Number: {controlNumber}");
				string extractedText = (string)dataReader["Extracted Text"];
				logger.LogInformation($"Extracted Text: {extractedText}");
				string nativeFileLocation = dataReader["NativeFileLocation"].ToString();
				logger.LogInformation($"NativeFileLocation: {nativeFileLocation}");
				long nativeFileSize = (long) dataReader["NativeFileSize"];
				logger.LogInformation($"NativeFileSize: {nativeFileSize}");
				string folderPath = (string) dataReader["FolderPath"];
				logger.LogInformation($"FolderPath: {folderPath}");
				int sourceWorkspace = (int)dataReader["Relativity Source Case"];
				logger.LogInformation($"Relativity Source Case: {sourceWorkspace}");
				int sourceJob = (int)dataReader["Relativity Source Job"];
				logger.LogInformation($"Relativity Source Job: {sourceJob}");

				logger.LogInformation("");
			}
		}
	}
}
