using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using kCura.Relativity.DataReaderClient;
using NUnit.Framework;
using Relativity.Services.Workspace;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Tests.System.Core;
using Relativity.Sync.Tests.System.Core.Helpers;
using Relativity.Sync.Utils;
using Relativity.Testing.Identification;
using ImportJobFactory = Relativity.Sync.Tests.System.Core.Helpers.ImportJobFactory;

namespace Relativity.Sync.Tests.System
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints.Sync]
	internal sealed class DestinationWorkspaceTagRepositoryTests : SystemTest
	{
		private int _sourceWorkspaceArtifactId;

		private readonly Guid _documentDestinationWorkspaceMultiObjectFieldGuid = new Guid("8980C2FA-0D33-4686-9A97-EA9D6F0B4196");
		private readonly Guid _documentJobHistoryMultiObjectFieldGuid = new Guid("97BC12FA-509B-4C75-8413-6889387D8EF6");

		protected override async Task ChildSuiteSetup()
		{
			WorkspaceRef workspace = await Environment.CreateWorkspaceWithFieldsAsync().ConfigureAwait(false);
			_sourceWorkspaceArtifactId = workspace.ArtifactID;
		}

		[IdentifiedTest("c5c953ed-0f70-4340-b44d-f3dfdda03b49")]
		public async Task ItShouldTagGivenDocumentsWithTheCorrectTag()
		{
			// Arrange
			const int numDocuments = 100;
			IList<int> documentsToTag = await UploadDocumentsAsync(numDocuments).ConfigureAwait(false);

			string jobHistoryName = Guid.NewGuid().ToString();
			int jobHistoryId = await Rdos.CreateJobHistoryInstanceAsync(ServiceFactory, _sourceWorkspaceArtifactId, jobHistoryName).ConfigureAwait(false);
			string destinationWorkspaceTagName = Guid.NewGuid().ToString();
			int destinationWorkspaceTagId = await Rdos.CreateDestinationWorkspaceTagInstanceAsync(ServiceFactory, _sourceWorkspaceArtifactId, 0, destinationWorkspaceTagName).ConfigureAwait(false);

			var configuration = new ConfigurationStub
			{
				SourceWorkspaceArtifactId = _sourceWorkspaceArtifactId,
				DestinationWorkspaceTagArtifactId = destinationWorkspaceTagId,
				JobHistoryArtifactId = jobHistoryId
			};

			// Act
			var repository = new DestinationWorkspaceTagRepository(new ServiceFactoryStub(ServiceFactory),
				new FederatedInstance(),
				new TagNameFormatter(new EmptyLogger()),
				configuration,
				new EmptyLogger(),
				new SyncMetrics(Enumerable.Empty<SyncMetricsSinkBase>(), new ConfigurationStub()),
				() => new StopwatchWrapper());

			IList<TagDocumentsResult<int>> results = await repository.TagDocumentsAsync(configuration, documentsToTag, CancellationToken.None).ConfigureAwait(false);

			// Assert
			Assert.IsNotNull(results);
			Assert.AreEqual(1, results.Count);
			TagDocumentsResult<int> result = results.First();

			Assert.IsTrue(result.Success, $"Failed to tag documents: {result.Message}");
			Assert.IsFalse(result.FailedDocuments.Any());
			Assert.AreEqual(documentsToTag.Count, result.TotalObjectsUpdated);

			string isTaggedCondition =
				$"('{_documentDestinationWorkspaceMultiObjectFieldGuid}' CONTAINS MULTIOBJECT [{destinationWorkspaceTagId}]) AND " +
				$"('{_documentJobHistoryMultiObjectFieldGuid}' CONTAINS MULTIOBJECT [{jobHistoryId}])";
			IList<int> taggedDocuments = await Rdos.QueryDocumentIdsAsync(ServiceFactory, _sourceWorkspaceArtifactId, isTaggedCondition).ConfigureAwait(false);
			CollectionAssert.AreEqual(documentsToTag.OrderBy(x => x), taggedDocuments.OrderBy(x => x));
		}

		private async Task<IList<int>> UploadDocumentsAsync(int numDocuments)
		{
			int destinationFolderId = await Rdos.GetRootFolderInstanceAsync(ServiceFactory, _sourceWorkspaceArtifactId).ConfigureAwait(false);
			ImportDataTableWrapper importDataTableWrapper = DataTableFactory.GenerateDocumentsWithExtractedText(numDocuments);

			ImportBulkArtifactJob documentImportJob = ImportJobFactory.CreateNonNativesDocumentImportJob(_sourceWorkspaceArtifactId, destinationFolderId, importDataTableWrapper);

			ImportJobErrors importErrors = await ImportJobExecutor.ExecuteAsync(documentImportJob).ConfigureAwait(false);
			Assert.IsTrue(importErrors.Success, $"{importErrors.Errors.Count} errors occurred during document upload: {importErrors}");

			IList<int> documentIds = await Rdos.QueryDocumentIdsAsync(ServiceFactory, _sourceWorkspaceArtifactId).ConfigureAwait(false);
			Assert.AreEqual(numDocuments, documentIds.Count, $"Unexpected number of documents in workspace {_sourceWorkspaceArtifactId}. Ensure test is run against clean workspace.");
			return documentIds;
		}
	}
}
