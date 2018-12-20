using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class CommandTests
	{
		private Command<IConfiguration> _command;

		private IConfiguration _configuration;
		private Mock<IExecutionConstrains<IConfiguration>> _executionConstrains;
		private Mock<IExecutor<IConfiguration>> _executor;

		[SetUp]
		public void SetUp()
		{
			_configuration = Mock.Of<IConfiguration>();
			_executionConstrains = new Mock<IExecutionConstrains<IConfiguration>>();
			_executor = new Mock<IExecutor<IConfiguration>>();

			_command = new Command<IConfiguration>(_configuration, _executionConstrains.Object, _executor.Object);
		}

		[Test]
		[TestCase(true)]
		[TestCase(false)]
		public async Task ItShouldCheckIfCommandCanBeExecuted(bool canExecute)
		{
			_executionConstrains.Setup(x => x.CanExecuteAsync(_configuration, CancellationToken.None)).ReturnsAsync(canExecute);

			// ACT
			bool result = await _command.CanExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			result.Should().Be(canExecute);
			_executionConstrains.Verify(x => x.CanExecuteAsync(_configuration, CancellationToken.None), Times.Once);
		}

		[Test]
		public async Task ItShouldExecuteCommand()
		{
			// ACT
			await _command.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_executor.Verify(x => x.ExecuteAsync(_configuration, CancellationToken.None));
		}

		[Test]
		public void ItShouldNotBlockExceptions()
		{
			_executor.Setup(x => x.ExecuteAsync(_configuration, CancellationToken.None)).Throws<Exception>();

			// ACT
			Func<Task> action = async () => await _command.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<Exception>();
		}
	}
}