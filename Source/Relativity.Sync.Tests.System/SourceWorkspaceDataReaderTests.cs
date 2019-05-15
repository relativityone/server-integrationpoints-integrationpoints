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
			const int sourceWorkspaceArtifactId = 1215252;
			const int dataSourceArtifactId = 1038052;
			const int controlNumberFieldId = 1003667;
			const int extractedTextFieldId = 1003668;
			const int folderInfoFieldId = 1035366;
			ConfigurationStub configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
				DataSourceArtifactId = dataSourceArtifactId,
				DestinationFolderStructureBehavior = DestinationFolderStructureBehavior.ReadFromField,
				FolderPathSourceFieldArtifactId = folderInfoFieldId,
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

			var sourceServiceFactory = new SourceServiceFactoryStub(ServiceFactory);
			var fieldManager = new FieldManager(configuration, sourceServiceFactory, new List<ISpecialFieldBuilder>
			{
				new FileInfoFieldsBuilder(new NativeFileRepository(sourceServiceFactory)),
				new FolderPathFieldBuilder(sourceServiceFactory, new FolderPathRetriever(sourceServiceFactory, new EmptyLogger()), configuration), new SourceTagsFieldBuilder()
			});
			var executor = new DataSourceSnapshotExecutor(sourceServiceFactory, fieldManager, new EmptyLogger());

			ExecutionResult result = await executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			Assert.AreEqual(ExecutionStatus.Completed, result.Status);

			const int resultsBlockSize = 100;
			SourceWorkspaceDataReader dataReader = new SourceWorkspaceDataReader(sourceServiceFactory, new SourceWorkspaceDataTableBuilder(fieldManager), new EmptyLogger(), configuration, resultsBlockSize);

			ConsoleLogger logger = new ConsoleLogger();
			object[] tmpTable = new object[resultsBlockSize];
			while (dataReader.Read())
			{
				for (int i = 0; i<dataReader.GetValues(tmpTable); i++)
				{
					logger.LogInformation($"{dataReader.GetName(i)} [{(tmpTable[i] == null ? "null" : tmpTable[i].GetType().Name)}]: {tmpTable[i]}");
				}

				logger.LogInformation("");
			}
		}
	}
}
