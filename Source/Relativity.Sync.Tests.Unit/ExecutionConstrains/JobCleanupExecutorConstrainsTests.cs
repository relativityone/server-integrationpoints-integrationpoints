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
	public class JobCleanupExecutorConstrainsTests
	{
		private JobCleanupExecutorConstrains _sut;

		[SetUp]
		public void SetUp()
		{
			_sut = new JobCleanupExecutorConstrains();
		}

		[Test]
		public async Task CanExecute_ShouldAlwaysReturnTrue()
		{
			// act
			bool canExecute = await _sut.CanExecuteAsync(Mock.Of<IJobCleanupConfiguration>(), CancellationToken.None).ConfigureAwait(false);

			// assert
			canExecute.Should().Be(true, $"JobCleanupExecutor should always execute, unless some previous step has failed. This behavior is defined by {nameof(PipelineBuilder)}.");
		}
	}
}