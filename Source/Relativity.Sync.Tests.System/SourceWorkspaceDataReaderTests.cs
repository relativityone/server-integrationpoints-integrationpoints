﻿using System;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
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
			const int sourceWorkspaceArtifactId = 1215252;
			const int dataSourceArtifactId = 1038052;
			const int controlNumberFieldId = 1003667;
			const int extractedTextFieldId = 1003668;
			const string folderInfoFieldName = "field name";
			ConfigurationStub configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = sourceWorkspaceArtifactId,
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

			var sourceServiceFactory = new ServiceFactoryStub(ServiceFactory);
			var documentFieldRepository = new DocumentFieldRepository(sourceServiceFactory, new EmptyLogger());
			var fieldManager = new FieldManager(configuration, documentFieldRepository, new List<ISpecialFieldBuilder>
			{
				new FileInfoFieldsBuilder(new NativeFileRepository(sourceServiceFactory)),
				new FolderPathFieldBuilder(new FolderPathRetriever(sourceServiceFactory, new EmptyLogger()), configuration), new SourceTagsFieldBuilder(configuration)
			});
			var executor = new DataSourceSnapshotExecutor(sourceServiceFactory, fieldManager, new JobProgressUpdaterFactory(sourceServiceFactory, configuration), new EmptyLogger());

			ExecutionResult result = await executor.ExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			Assert.AreEqual(ExecutionStatus.Completed, result.Status);

			SourceWorkspaceDataReader dataReader = BuildDataReader(fieldManager, configuration, sourceServiceFactory);
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

		private static SourceWorkspaceDataReader BuildDataReader(IFieldManager fieldManager, ISynchronizationConfiguration configuration, ServiceFactoryStub sourceServiceFactory)
		{
			IRelativityExportBatcher ExportBatcherFactory(Guid runId, int workspaceArtifactId, int syncConfigurationArtifactId)
			{
				return new RelativityExportBatcher(sourceServiceFactory, new BatchRepository(sourceServiceFactory), runId, workspaceArtifactId, syncConfigurationArtifactId);
			}

			SourceWorkspaceDataReader dataReader = new SourceWorkspaceDataReader(new BatchDataReaderBuilder(fieldManager),
				configuration,
				ExportBatcherFactory,
				fieldManager,
				new ItemStatusMonitor(),
				new EmptyLogger());
			return dataReader;
		}
	}
}
