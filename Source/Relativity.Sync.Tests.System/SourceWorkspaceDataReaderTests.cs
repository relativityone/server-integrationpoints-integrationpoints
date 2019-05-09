using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Tests.Common;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

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
			//const int numDocs = 2;
			//int folderId = await Rdos.GetRootFolderInstance(ServiceFactory, _sourceWorkspaceArtifactId).ConfigureAwait(false);
			//DocumentData data = DocumentData.GenerateDocumentsWithoutNatives(numDocs);
			//ImportBulkArtifactJob job = ImportJobFactory.CreateNonNativesDocumentImportJob(_sourceWorkspaceArtifactId, folderId, data);
			//ImportJobResult jobResult = await ImportJobExecutor.ExecuteAsync(job).ConfigureAwait(false);

			//Assert.IsTrue(jobResult.Success);

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

			SourceWorkspaceDataReader dataReader = new SourceWorkspaceDataReader(sourceServiceFactory, sourceWorkspaceArtifactId, configuration.ExportRunId, 1, configuration.FieldMappings, new EmptyLogger());


			while (dataReader.Read())
			{
				string controlNumber = (string)dataReader["Control Number"];
				string extractedText = (string)dataReader["Extracted Text"];
			}
		}
	}
}
