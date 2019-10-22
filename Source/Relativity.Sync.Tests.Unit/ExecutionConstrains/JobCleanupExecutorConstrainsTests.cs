using System;
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
		private Mock<IJobCleanupConfiguration> _fakeConfiguration;
		private JobCleanupExecutorConstrains _sut;
		
		[SetUp]
		public void SetUp()
		{
			_fakeConfiguration = new Mock<IJobCleanupConfiguration>();
			_sut = new JobCleanupExecutorConstrains();
		}

		[Test]
		public async Task CanExecute_ShouldReturnTrue_WhenSynchronizationCompletedWithSuccess()
		{
			_fakeConfiguration.SetupGet(x => x.SynchronizationExecutionResult).Returns(ExecutionResult.Success);

			// act
			bool canExecute = await _sut.CanExecuteAsync(_fakeConfiguration.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			canExecute.Should().Be(true);
		}

		[Test]
		public async Task CanExecute_ShouldReturnFalse_WhenSynchronizationFailed()
		{
			_fakeConfiguration.SetupGet(x => x.SynchronizationExecutionResult).Returns(ExecutionResult.Failure(new InvalidOperationException()));

			// act
			bool canExecute = await _sut.CanExecuteAsync(_fakeConfiguration.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			canExecute.Should().Be(false);
		}

		[Test]
		public async Task CanExecute_ShouldReturnFalse_WhenSynchronizationCompletedWithErrors()
		{
			_fakeConfiguration.SetupGet(x => x.SynchronizationExecutionResult).Returns(ExecutionResult.SuccessWithErrors(new InvalidOperationException()));

			// act
			bool canExecute = await _sut.CanExecuteAsync(_fakeConfiguration.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			canExecute.Should().Be(false);
		}

		[Test]
		public async Task CanExecute_ShouldReturnFalse_WhenSynchronizationSkipped()
		{
			_fakeConfiguration.SetupGet(x => x.SynchronizationExecutionResult).Returns(ExecutionResult.Skipped);

			// act
			bool canExecute = await _sut.CanExecuteAsync(_fakeConfiguration.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			canExecute.Should().Be(false);
		}

		[Test]
		public async Task CanExecute_ShouldReturnFalse_WhenSynchronizationCanceled()
		{
			_fakeConfiguration.SetupGet(x => x.SynchronizationExecutionResult).Returns(ExecutionResult.Canceled);

			// act
			bool canExecute = await _sut.CanExecuteAsync(_fakeConfiguration.Object, CancellationToken.None).ConfigureAwait(false);

			// assert
			canExecute.Should().Be(false);
		}
	}
}