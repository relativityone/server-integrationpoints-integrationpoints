using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Internal;
using FluentAssertions;
using kCura.Vendor.Castle.Components.DictionaryAdapter.Xml;
using Moq;
using NUnit.Framework;
using Relativity.Services.DataContracts.DTOs.Results;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	internal sealed class DocumentDataSourceSnapshotExecutorTests
	{
		private DocumentDataSourceSnapshotExecutor _instance;

		private Mock<IObjectManager> _objectManager;
		private Mock<IDocumentDataSourceSnapshotConfiguration> _configuration;
		private Mock<IJobProgressUpdater> _jobProgressUpdater;
		private Mock<INativeFileRepository> _nativeFileRepository;
		private IJobStatisticsContainer _jobStatisticsContainer;
		private Mock<IFieldManager> _fieldManager;

		private const int _WORKSPACE_ID = 458712;
		private const int _DATA_SOURCE_ID = 485219;

		[SetUp]
		public void SetUp()
		{
			_objectManager = new Mock<IObjectManager>();

			Mock<ISourceServiceFactoryForUser> serviceFactory = new Mock<ISourceServiceFactoryForUser>();
			serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);

			_fieldManager = new Mock<IFieldManager>();
			_fieldManager.Setup(fm => fm.GetDocumentTypeFieldsAsync(CancellationToken.None)).ReturnsAsync(Mock.Of<List<FieldInfoDto>>());

			_configuration = new Mock<IDocumentDataSourceSnapshotConfiguration>();
			_configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(_WORKSPACE_ID);
			_configuration.Setup(x => x.DataSourceArtifactId).Returns(_DATA_SOURCE_ID);
			_configuration.Setup(x => x.GetFieldMappings()).Returns(new List<FieldMap>());

			_nativeFileRepository = new Mock<INativeFileRepository>();
			_jobStatisticsContainer = new JobStatisticsContainer();

			_jobProgressUpdater = new Mock<IJobProgressUpdater>();
			Mock<IJobProgressUpdaterFactory> jobProgressUpdaterFactory = new Mock<IJobProgressUpdaterFactory>();
			jobProgressUpdaterFactory.Setup(x => x.CreateJobProgressUpdater()).Returns(_jobProgressUpdater.Object);

			_instance = new DocumentDataSourceSnapshotExecutor(serviceFactory.Object, _fieldManager.Object, jobProgressUpdaterFactory.Object,
				_nativeFileRepository.Object, _jobStatisticsContainer, new EmptyLogger());
		}

		[Test]
		public async Task ItShouldSetNativesSize()
		{
#pragma warning disable RG2009 // Hardcoded Numeric Value
			// Arrange
			const long expectedNativesSize = 50;

			ExportInitializationResults exportInitializationResults = new ExportInitializationResults();

			_nativeFileRepository
				.Setup(x => x.CalculateNativesTotalSizeAsync(It.IsAny<int>(), It.IsAny<QueryRequest>()))
				.Returns(Task.FromResult(expectedNativesSize));

			_objectManager.Setup(x => x.InitializeExportAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1)).ReturnsAsync(exportInitializationResults);

			// Act
			await _instance.ExecuteAsync(_configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);
			long nativesSize = await _jobStatisticsContainer.NativesBytesRequested.ConfigureAwait(false);

			// Assert
			nativesSize.Should().Be(expectedNativesSize);
#pragma warning restore RG2009 // Hardcoded Numeric Value
		}

		[Test]
		public async Task ItShouldInitializeExportAndSaveResult()
		{
			const int totalRecords = 123456789;
			Guid runId = Guid.NewGuid();

			ExportInitializationResults exportInitializationResults = new ExportInitializationResults
			{
				RecordCount = totalRecords,
				RunID = runId
			};
			_objectManager.Setup(x => x.InitializeExportAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1)).ReturnsAsync(exportInitializationResults);
			_objectManager.Setup(x => x.RetrieveResultsBlockFromExportAsync(It.IsAny<int>(), It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<int>())).ReturnsAsync(Array.Empty<RelativityObjectSlim>());
			_nativeFileRepository.Setup(x => x.QueryAsync(It.IsAny<int>(), It.IsAny<ICollection<int>>())).ReturnsAsync(Enumerable.Empty<INativeFile>());

			// ACT
			ExecutionResult result = await _instance.ExecuteAsync(_configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// ASSERT
			result.Status.Should().Be(ExecutionStatus.Completed);
			_objectManager.Verify(x => x.InitializeExportAsync(_WORKSPACE_ID, It.Is<QueryRequest>(qr => AssertQueryRequest(qr)), 1));
			_configuration.Verify(x => x.SetSnapshotDataAsync(runId, totalRecords));
			_jobProgressUpdater.Verify(x => x.SetTotalItemsCountAsync(totalRecords));
		}

		private bool AssertQueryRequest(QueryRequest queryRequest)
		{
			const int documentArtifactTypeId = (int) ArtifactType.Document;
			queryRequest.ObjectType.ArtifactTypeID.Should().Be(documentArtifactTypeId);

			queryRequest.Condition.Should().Be($"(('ArtifactId' IN SAVEDSEARCH {_DATA_SOURCE_ID}))");
			return true;
		}

		[Test]
		public async Task ItShouldFailWhenExportApiFails()
		{
			_objectManager.Setup(x => x.InitializeExportAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1)).Throws<InvalidOperationException>();

			// ACT
			ExecutionResult executionResult = await _instance.ExecuteAsync(_configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// ASSERT
			executionResult.Status.Should().Be(ExecutionStatus.Failed);
			executionResult.Exception.Should().BeOfType<InvalidOperationException>();
		}

		[Test]
		[TestCase(DestinationFolderStructureBehavior.None)]
		[TestCase(DestinationFolderStructureBehavior.RetainSourceWorkspaceStructure)]
		public async Task ItShouldNotIncludeFolderPathSourceField(DestinationFolderStructureBehavior destinationFolderStructureBehavior)
		{
			const string folderPathSourceFieldName = "folder path";

			ExportInitializationResults exportInitializationResults = new ExportInitializationResults
			{
				RecordCount = 1L,
				RunID = Guid.NewGuid()
			};
			_objectManager.Setup(x => x.InitializeExportAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1)).ReturnsAsync(exportInitializationResults);

			// ACT
			await _instance.ExecuteAsync(_configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_objectManager.Verify(x => x.InitializeExportAsync(_WORKSPACE_ID, It.Is<QueryRequest>(qr => AssertNotIncludingFolderPathSourceField(qr, folderPathSourceFieldName)), 1));
		}

		private bool AssertNotIncludingFolderPathSourceField(QueryRequest queryRequest, string folderPathSourceFieldName)
		{
			queryRequest.Fields.Should().NotContain(x => x.Name == folderPathSourceFieldName);
			return true;
		}

		[Test]
		public async Task ItShouldIncludeFieldsFromFieldMapping()
		{
			const string field1Id = "741258";
			const string field1DestId = "Dest 1";
			const string field2Id = "985632";
			const string field2DestId = "Dest 2";

			List<FieldInfoDto> fieldInfos = new List<FieldInfoDto>
			{
				FieldInfoDto.DocumentField(field1Id, field1DestId, false),
				FieldInfoDto.DocumentField(field2Id, field2DestId, false)
			};

			_fieldManager.Setup(fm => fm.GetDocumentTypeFieldsAsync(CancellationToken.None)).ReturnsAsync(fieldInfos);

			ExportInitializationResults exportInitializationResults = new ExportInitializationResults
			{
				RecordCount = 1L,
				RunID = Guid.NewGuid()
			};
			_objectManager.Setup(x => x.InitializeExportAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1)).ReturnsAsync(exportInitializationResults);

			// ACT
			await _instance.ExecuteAsync(_configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_objectManager.Verify(x => x.InitializeExportAsync(_WORKSPACE_ID, It.Is<QueryRequest>(qr => AssertFieldMapping(qr, field1Id, field2Id)), 1));
		}

		private bool AssertFieldMapping(QueryRequest queryRequest, string field1Name, string field2Name)
		{
			if (queryRequest.Fields.IsNullOrEmpty())
			{
				return false;
			}

			queryRequest.Fields.Should().Contain(x => x.Name == field1Name);
			queryRequest.Fields.Should().Contain(x => x.Name == field2Name);
			return true;
		}
	}
}