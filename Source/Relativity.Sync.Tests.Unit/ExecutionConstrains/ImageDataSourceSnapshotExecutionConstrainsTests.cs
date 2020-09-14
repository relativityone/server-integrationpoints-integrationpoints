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
	public sealed class ImageDataSourceSnapshotExecutionConstrainsTests
	{
		private ImageDataSourceSnapshotExecutionConstrains _instance;

		[SetUp]
		public void SetUp()
		{
			_instance = new ImageDataSourceSnapshotExecutionConstrains();
		}

		[TestCase(true, false)]
		[TestCase(false, true)]
		public async Task ItShouldPreventExecutionIfSnapshotExists(bool snapshotExists, bool expectedCanExecute)
		{
			Mock<IImageDataSourceSnapshotConfiguration> configuration = new Mock<IImageDataSourceSnapshotConfiguration>();

			configuration.Setup(x => x.IsSnapshotCreated).Returns(snapshotExists);

			// ACT
			bool canExecute = await _instance.CanExecuteAsync(configuration.Object, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			canExecute.Should().Be(expectedCanExecute);
		}
	}
}