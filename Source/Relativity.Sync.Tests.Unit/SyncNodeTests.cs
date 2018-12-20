using System;
using System.Threading;
using System.Threading.Tasks;
using Banzai;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Tests.Unit.Stubs;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class SyncNodeTests
	{
		private SyncNodeStub _instance;

		private SyncExecutionContext _executionContext;

		private Mock<ICommand<IConfiguration>> _command;

		private const string _STEP_NAME = "step name";

		[SetUp]
		public void SetUp()
		{
			_executionContext = new SyncExecutionContext(new Progress<SyncProgress>(), CancellationToken.None);

			_command = new Mock<ICommand<IConfiguration>>();

			_instance = new SyncNodeStub(_command.Object, new EmptyLogger(), _STEP_NAME);
		}

		[Test]
		public async Task ItShouldExecuteCommand()
		{
			_command.Setup(x => x.CanExecuteAsync(CancellationToken.None)).ReturnsAsync(true);

			// ACT
			await _instance.ExecuteAsync(_executionContext).ConfigureAwait(false);

			// ASSERT
			_command.Verify(x => x.ExecuteAsync(CancellationToken.None), Times.Once);
		}

		[Test]
		public async Task ItShouldNotExecuteCommandWhenUnableTo()
		{
			_command.Setup(x => x.CanExecuteAsync(CancellationToken.None)).ReturnsAsync(false);

			// ACT
			await _instance.ExecuteAsync(_executionContext).ConfigureAwait(false);

			// ASSERT
			_command.Verify(x => x.ExecuteAsync(CancellationToken.None), Times.Never);
		}

		[Test]
		public void ItShouldNotCatchExceptions()
		{
			_command.Setup(x => x.CanExecuteAsync(CancellationToken.None)).ReturnsAsync(true);
			_command.Setup(x => x.ExecuteAsync(CancellationToken.None)).Throws<Exception>();

			ExecutionOptions options = new ExecutionOptions
			{
				ThrowOnError = true
			};
			SyncNodeStub instance = new SyncNodeStub(options, _command.Object, new EmptyLogger(), _STEP_NAME);

			// ACT
			Func<Task> action = async () => await instance.ExecuteAsync(_executionContext).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<Exception>();
		}
	}
}