using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;
using Relativity.Sync.Logging;
using Relativity.Sync.Storage;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	internal sealed class SnapshotPartitionExecutionConstrainsTests
	{
		private SnapshotPartitionExecutionConstrains _instance;

		private Mock<IBatchRepository> _batchRepository;
		private ISnapshotPartitionConfiguration _configuration;

		private const int _WORKSPACE_ID = 986574;
		private const int _SYNC_CONF_ID = 365298;

		[SetUp]
		public void SetUp()
		{
			_batchRepository = new Mock<IBatchRepository>();

			Mock<ISnapshotPartitionConfiguration> configurationMock = new Mock<ISnapshotPartitionConfiguration>();
			configurationMock.Setup(x => x.SourceWorkspaceArtifactId).Returns(_WORKSPACE_ID);
			configurationMock.Setup(x => x.SyncConfigurationArtifactId).Returns(_SYNC_CONF_ID);

			_configuration = configurationMock.Object;

			_instance = new SnapshotPartitionExecutionConstrains(_batchRepository.Object, new EmptyLogger());
		}

		[Test]
		public async Task ItShouldExecuteWhenBatchesAreMissing()
		{
			_batchRepository.Setup(x => x.AreBatchesCreated(_WORKSPACE_ID, _SYNC_CONF_ID)).ReturnsAsync(false);

			// ACT
			bool shouldExecute = await _instance.CanExecuteAsync(_configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			shouldExecute.Should().BeTrue();
		}

		[Test]
		public async Task ItShouldPreventExecutionWhenBatchesExist()
		{
			_batchRepository.Setup(x => x.AreBatchesCreated(_WORKSPACE_ID, _SYNC_CONF_ID)).ReturnsAsync(true);

			// ACT
			bool shouldExecute = await _instance.CanExecuteAsync(_configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			shouldExecute.Should().BeFalse();
		}

		[Test]
		public void ItShouldNotHideException()
		{
			_batchRepository.Setup(x => x.AreBatchesCreated(_WORKSPACE_ID, _SYNC_CONF_ID)).Throws<InvalidOperationException>();

			// ACT
			Func<Task> action = async () => await _instance.CanExecuteAsync(_configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<InvalidOperationException>();
		}
	}
}