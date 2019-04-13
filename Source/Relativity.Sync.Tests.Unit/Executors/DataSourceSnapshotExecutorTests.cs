using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	internal sealed class DataSourceSnapshotExecutorTests
	{
		private DataSourceSnapshotExecutor _instance;

		private Mock<IObjectManager> _objectManager;
		private Mock<IDataSourceSnapshotConfiguration> _configuration;

		private const int _WORKSPACE_ID = 458712;
		private const int _DATA_SOURCE_ID = 485219;

		[SetUp]
		public void SetUp()
		{
			_objectManager = new Mock<IObjectManager>();

			Mock<ISourceServiceFactoryForUser> serviceFactory = new Mock<ISourceServiceFactoryForUser>();
			serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);

			_configuration = new Mock<IDataSourceSnapshotConfiguration>();
			_configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(_WORKSPACE_ID);
			_configuration.Setup(x => x.DataSourceArtifactId).Returns(_DATA_SOURCE_ID);

			_instance = new DataSourceSnapshotExecutor(serviceFactory.Object, new EmptyLogger());
		}

		[Test]
		public async Task ItShouldInitializeExportAndSaveResult()
		{
			const long totalRecords = 123456789;
			Guid runId = Guid.NewGuid();

			ExportInitializationResults exportInitializationResults = new ExportInitializationResults
			{
				RecordCount = totalRecords,
				RunID = runId
			};
			_objectManager.Setup(x => x.InitializeExportAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1)).ReturnsAsync(exportInitializationResults);

			// ACT
			ExecutionResult result = await _instance.ExecuteAsync(_configuration.Object, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			result.Status.Should().Be(ExecutionStatus.Completed);
			_objectManager.Verify(x => x.InitializeExportAsync(_WORKSPACE_ID, It.Is<QueryRequest>(qr => AssertQueryRequest(qr)), 1));
			_configuration.Verify(x => x.SetSnapshotDataAsync(runId, totalRecords));
		}

		private bool AssertQueryRequest(QueryRequest queryRequest)
		{
			const int documentArtifactTypeId = 10;
			queryRequest.ObjectType.ArtifactTypeID.Should().Be(documentArtifactTypeId);

			queryRequest.Condition.Should().Be($"(('ArtifactId' IN SAVEDSEARCH {_DATA_SOURCE_ID}))");
			return true;
		}

		[Test]
		public async Task ItShouldFailWhenExportApiFails()
		{
			_objectManager.Setup(x => x.InitializeExportAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1)).Throws<InvalidOperationException>();

			// ACT
			ExecutionResult executionResult = await _instance.ExecuteAsync(_configuration.Object, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			executionResult.Status.Should().Be(ExecutionStatus.Failed);
			executionResult.Exception.Should().BeOfType<InvalidOperationException>();
		}

		[Test]
		public async Task ItShouldIncludeSystemFields()
		{
			ExportInitializationResults exportInitializationResults = new ExportInitializationResults
			{
				RecordCount = 1L,
				RunID = Guid.NewGuid()
			};
			_objectManager.Setup(x => x.InitializeExportAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1)).ReturnsAsync(exportInitializationResults);

			// ACT
			await _instance.ExecuteAsync(_configuration.Object, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_objectManager.Verify(x => x.InitializeExportAsync(_WORKSPACE_ID, It.Is<QueryRequest>(qr => AssertSystemFields(qr)), 1));
		}

		private bool AssertSystemFields(QueryRequest queryRequest)
		{
			const string supportedByViewerFieldName = "SupportedByViewer";
			const string relativityNativeTypeFieldName = "RelativityNativeType";

			queryRequest.Fields.Should().Contain(x => x.Name == supportedByViewerFieldName);
			queryRequest.Fields.Should().Contain(x => x.Name == relativityNativeTypeFieldName);
			return true;
		}

		[Test]
		public async Task ItShouldIncludeFolderPathSourceField()
		{
			const int folderPathSourceFieldId = 589632;

			ExportInitializationResults exportInitializationResults = new ExportInitializationResults
			{
				RecordCount = 1L,
				RunID = Guid.NewGuid()
			};
			_objectManager.Setup(x => x.InitializeExportAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1)).ReturnsAsync(exportInitializationResults);

			_configuration.Setup(x => x.DestinationFolderStructureBehavior).Returns(DestinationFolderStructureBehavior.ReadFromField);
			_configuration.Setup(x => x.FolderPathSourceFieldArtifactId).Returns(folderPathSourceFieldId);

			// ACT
			await _instance.ExecuteAsync(_configuration.Object, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_objectManager.Verify(x => x.InitializeExportAsync(_WORKSPACE_ID, It.Is<QueryRequest>(qr => AssertFolderPathSourceField(qr, folderPathSourceFieldId)), 1));
		}

		private bool AssertFolderPathSourceField(QueryRequest queryRequest, int folderPathSourceFieldId)
		{
			queryRequest.Fields.Should().Contain(x => x.ArtifactID == folderPathSourceFieldId);
			return true;
		}

		[Test]
		[TestCase(DestinationFolderStructureBehavior.None)]
		[TestCase(DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure)]
		public async Task ItShouldNotIncludeFolderPathSourceField(DestinationFolderStructureBehavior destinationFolderStructureBehavior)
		{
			const int folderPathSourceFieldId = 589632;

			ExportInitializationResults exportInitializationResults = new ExportInitializationResults
			{
				RecordCount = 1L,
				RunID = Guid.NewGuid()
			};
			_objectManager.Setup(x => x.InitializeExportAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1)).ReturnsAsync(exportInitializationResults);

			_configuration.Setup(x => x.DestinationFolderStructureBehavior).Returns(destinationFolderStructureBehavior);
			_configuration.Setup(x => x.FolderPathSourceFieldArtifactId).Returns(folderPathSourceFieldId);

			// ACT
			await _instance.ExecuteAsync(_configuration.Object, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_objectManager.Verify(x => x.InitializeExportAsync(_WORKSPACE_ID, It.Is<QueryRequest>(qr => AssertNotIncludingFolderPathSourceField(qr, folderPathSourceFieldId)), 1));
		}

		private bool AssertNotIncludingFolderPathSourceField(QueryRequest queryRequest, int folderPathSourceFieldId)
		{
			queryRequest.Fields.Should().NotContain(x => x.ArtifactID == folderPathSourceFieldId);
			return true;
		}
	}
}