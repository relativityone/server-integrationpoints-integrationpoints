using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;

namespace Relativity.Sync.Tests.Unit.ExecutionConstrains
{
	[TestFixture]
	public sealed class DataSourceSnapshotExecutionConstrainsTests
	{
		private DataSourceSnapshotExecutionConstrains _instance;

		[SetUp]
		public void SetUp()
		{
			_instance = new DataSourceSnapshotExecutionConstrains();
		}

		[Test]
		public async Task ItShouldPreventExecutionIfSnapshotExists()
		{
			Mock<IDataSourceSnapshotConfiguration> configuration = new Mock<IDataSourceSnapshotConfiguration>();

			configuration.Setup(x => x.IsSnapshotCreated).Returns(true);

			// ACT
			bool canExecute = await _instance.CanExecuteAsync(configuration.Object, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			canExecute.Should().BeFalse();
		}

		[Test]
		public async Task ItShouldAllowExecutionIfSnapshotDoesNotExist()
		{
			Mock<IDataSourceSnapshotConfiguration> configuration = new Mock<IDataSourceSnapshotConfiguration>();

			configuration.Setup(x => x.IsSnapshotCreated).Returns(false);

			// ACT
			bool canExecute = await _instance.CanExecuteAsync(configuration.Object, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			canExecute.Should().BeTrue();
		}
	}
}