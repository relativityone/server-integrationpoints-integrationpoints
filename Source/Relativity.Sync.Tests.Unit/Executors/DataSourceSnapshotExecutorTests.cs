using System;
using System.Collections.Generic;
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
using Relativity.Sync.Storage;
using Relativity.Sync.Transfer;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	internal sealed class DataSourceSnapshotExecutorTests
	{
		private DataSourceSnapshotExecutor _instance;

		private Mock<IObjectManager> _objectManager;
		private Mock<IDataSourceSnapshotConfiguration> _configuration;
		private Mock<IJobProgressUpdater> _jobProgressUpdater;
		private Mock<ISnapshotQueryRequestProvider> _snapshotQueryRequestProviderFake;

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
			_configuration.Setup(x => x.GetFieldMappings()).Returns(new List<FieldMap>());

			_snapshotQueryRequestProviderFake = new Mock<ISnapshotQueryRequestProvider>();

			_jobProgressUpdater = new Mock<IJobProgressUpdater>();
			Mock<IJobProgressUpdaterFactory> jobProgressUpdaterFactory = new Mock<IJobProgressUpdaterFactory>();
			jobProgressUpdaterFactory.Setup(x => x.CreateJobProgressUpdater()).Returns(_jobProgressUpdater.Object);

			_instance = new DataSourceSnapshotExecutor(serviceFactory.Object, jobProgressUpdaterFactory.Object,
				new EmptyLogger(), _snapshotQueryRequestProviderFake.Object);
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

			// ACT
			ExecutionResult result = await _instance.ExecuteAsync(_configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);

			// ASSERT
			result.Status.Should().Be(ExecutionStatus.Completed);
			_objectManager.Verify(x => x.InitializeExportAsync(_WORKSPACE_ID, It.IsAny<QueryRequest>(), 1));
			_configuration.Verify(x => x.SetSnapshotDataAsync(runId, totalRecords));
			_jobProgressUpdater.Verify(x => x.SetTotalItemsCountAsync(totalRecords));
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
	}
}