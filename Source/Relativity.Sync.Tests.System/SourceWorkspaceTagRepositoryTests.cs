using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Relativity.Sync.Utils;
using kCura.Relativity.DataReaderClient;
using Moq;
using NUnit.Framework;
using Relativity.Services.Workspace;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Testing.Identification;
using ImportJobFactory = Relativity.Sync.Tests.System.Core.Helpers.ImportJobFactory;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.Sync]
	internal sealed class SourceWorkspaceTagRepositoryTests : SystemTest
	{
		private int _destinationWorkspaceArtifactId;

		private readonly Guid _sourceWorkspaceTagFieldMultiObject = new Guid("2FA844E3-44F0-47F9-ABB7-D6D8BE0C9B8F");
		private readonly Guid _sourceJobTagFieldMultiObject = new Guid("7CC3FAAF-CBB8-4315-A79F-3AA882F1997F");

		protected override async Task ChildSuiteSetup()
		{
			WorkspaceRef workspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
			_destinationWorkspaceArtifactId = workspace.ArtifactID;
		}

		[IdentifiedTest("FA231B33-24EB-42C7-8560-597B5867441B")]
		public async Task ItShouldTagGivenDocumentsWithTheCorrectTag()
		{
			// Arrange
			const int numDocuments = 100;
			IList<string> documentsToTag = await UploadDocumentsAsync(numDocuments).ConfigureAwait(false);

			const int testSourceCaseArtifactId = 17890987;
			const string testSourceInstanceName = "This wonderful instance";
			const string testSourceCaseName = "My ECA Workspace";
			var sourceCaseTag = new RelativitySourceCaseTag
			{
				SourceWorkspaceArtifactId = testSourceCaseArtifactId,
				SourceInstanceName = testSourceInstanceName,
				SourceWorkspaceName = testSourceCaseName,
				Name = $"{testSourceInstanceName} - {testSourceCaseName} - {testSourceCaseArtifactId}"
			};
			int sourceCaseTagId = await Rdos.CreateRelativitySourceCaseInstanceAsync(ServiceFactory, _destinationWorkspaceArtifactId, sourceCaseTag).ConfigureAwait(false);

			const int testJobHistoryArtifactId = 18880999;
			const string testJobHistoryName = "My originating job history 1";
			var sourceJobTag = new RelativitySourceJobTag
			{
				JobHistoryArtifactId = testJobHistoryArtifactId,
				JobHistoryName = testJobHistoryName,
				Name = $"{testJobHistoryName} - {testJobHistoryArtifactId}",
				SourceCaseTagArtifactId = sourceCaseTagId
			};
			int sourceJobTagId = await Rdos.CreateRelativitySourceJobInstanceAsync(ServiceFactory, _destinationWorkspaceArtifactId, sourceJobTag).ConfigureAwait(false);

			const string identifierFieldName = "Control Number";
			var fieldMap = new List<FieldMap>
			{
				new FieldMap
				{
					SourceField = new FieldEntry
					{
						DisplayName = identifierFieldName,
						IsIdentifier = true
					},
					DestinationField = new FieldEntry
					{
						DisplayName = identifierFieldName,
						IsIdentifier = true
					}
				}
			};

			var configuration = new ConfigurationStub
			{
				DestinationWorkspaceArtifactId = _destinationWorkspaceArtifactId,
				SourceWorkspaceTagArtifactId = sourceCaseTagId,
				SourceJobTagArtifactId = sourceJobTagId,
				SourceWorkspaceArtifactId = testSourceCaseArtifactId
			};
			configuration.SetFieldMappings(fieldMap);


			var serviceFactoryStub = new ServiceFactoryStub(ServiceFactory);
			var logger = new EmptyLogger();
			Mock<IFieldMappings> fieldMappings = new Mock<IFieldMappings>(MockBehavior.Strict);
			fieldMappings.Setup(x => x.GetFieldMappings()).Returns(fieldMap);

			// Act
			var repository = new SourceWorkspaceTagRepository(serviceFactoryStub, logger,
				new SyncMetrics(Enumerable.Empty<SyncMetricsSinkBase>(), new ConfigurationStub()),
				fieldMappings.Object, () => new StopwatchWrapper());

			IList<TagDocumentsResult<string>> results = await repository.TagDocumentsAsync(configuration, documentsToTag, CancellationToken.None).ConfigureAwait(false);

			// Assert
			Assert.IsNotNull(results);
			Assert.AreEqual(1, results.Count);
			TagDocumentsResult<string> result = results.First();

			Assert.IsTrue(result.Success, $"Failed to tag documents: {result.Message}");
			Assert.IsFalse(result.FailedDocuments.Any());
			Assert.AreEqual(documentsToTag.Count, result.TotalObjectsUpdated);

			string isTaggedCondition =
				$"('{_sourceWorkspaceTagFieldMultiObject}' CONTAINS MULTIOBJECT [{sourceCaseTagId}]) AND " +
				$"('{_sourceJobTagFieldMultiObject}' CONTAINS MULTIOBJECT [{sourceJobTagId}])";
			IList<string> taggedDocuments = await Rdos.QueryDocumentNamesAsync(ServiceFactory, _destinationWorkspaceArtifactId, isTaggedCondition).ConfigureAwait(false);
			CollectionAssert.AreEqual(documentsToTag.OrderBy(x => x), taggedDocuments.OrderBy(x => x));
		}

		private async Task<IList<string>> UploadDocumentsAsync(int numDocuments)
		{
			int destinationFolderId = await Rdos.GetRootFolderInstanceAsync(ServiceFactory, _destinationWorkspaceArtifactId).ConfigureAwait(false);
			ImportDataTableWrapper importDataTableWrapper = DataTableFactory.GenerateDocumentsWithExtractedText(numDocuments);

			ImportBulkArtifactJob documentImportJob = ImportJobFactory.CreateNonNativesDocumentImportJob(_destinationWorkspaceArtifactId, destinationFolderId, importDataTableWrapper);

			ImportJobErrors importErrors = await ImportJobExecutor.ExecuteAsync(documentImportJob).ConfigureAwait(false);
			Assert.IsTrue(importErrors.Success, $"{importErrors.Errors.Count} errors occurred during document upload: {importErrors}");

			IList<string> documentIds = await Rdos.GetAllDocumentNamesAsync(ServiceFactory, _destinationWorkspaceArtifactId).ConfigureAwait(false);
			Assert.AreEqual(numDocuments, documentIds.Count, $"Unexpected number of documents in workspace {_destinationWorkspaceArtifactId}. Ensure test is run against clean workspace.");
			return documentIds;
		}
	}
}