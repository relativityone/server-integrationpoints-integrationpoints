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
	public sealed class DataSourceSnapshotExecutorTests
	{
		private DataSourceSnapshotExecutor _instance;

		private Mock<IObjectManager> _objectManager;

		private const int _WORKSPACE_ID = 458712;

		[SetUp]
		public void SetUp()
		{
			_objectManager = new Mock<IObjectManager>();

			Mock<ISourceServiceFactoryForUser> serviceFactory = new Mock<ISourceServiceFactoryForUser>();
			serviceFactory.Setup(x => x.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);

			_instance = new DataSourceSnapshotExecutor(serviceFactory.Object, new EmptyLogger());
		}

		[Test]
		public async Task ItShouldInitializeExportAndSaveResult()
		{
			const int savedSearchId = 458967;

			const long totalRecords = 123456789;
			Guid runId = Guid.NewGuid();

			Mock<IDataSourceSnapshotConfiguration> configuration = new Mock<IDataSourceSnapshotConfiguration>();
			configuration.Setup(x => x.DataSourceArtifactId).Returns(savedSearchId);
			configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(_WORKSPACE_ID);

			ExportInitializationResults exportInitializationResults = new ExportInitializationResults
			{
				RecordCount = totalRecords,
				RunID = runId
			};
			_objectManager.Setup(x => x.InitializeExportAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1)).ReturnsAsync(exportInitializationResults);

			// ACT
			await _instance.ExecuteAsync(configuration.Object, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_objectManager.Verify(x => x.InitializeExportAsync(_WORKSPACE_ID, It.Is<QueryRequest>(qr => AssertQueryRequest(qr, savedSearchId)), 1));
			configuration.Verify(x => x.SetSnapshotDataAsync(runId, totalRecords));
		}

		private bool AssertQueryRequest(QueryRequest queryRequest, int savedSearchArtifactId)
		{
			const int documentArtifactTypeId = 10;
			queryRequest.ObjectType.ArtifactTypeID.Should().Be(documentArtifactTypeId);

			queryRequest.Condition.Should().Be($"(('ArtifactId' IN SAVEDSEARCH {savedSearchArtifactId}))");
			return true;
		}

		[Test]
		public void ItShouldFailWhenExportApiFails()
		{
			_objectManager.Setup(x => x.InitializeExportAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1)).Throws<InvalidOperationException>();

			Mock<IDataSourceSnapshotConfiguration> configuration = new Mock<IDataSourceSnapshotConfiguration>();
			configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(_WORKSPACE_ID);

			// ACT
			Func<Task> action = async () => await _instance.ExecuteAsync(configuration.Object, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<InvalidOperationException>();
		}
	}
}