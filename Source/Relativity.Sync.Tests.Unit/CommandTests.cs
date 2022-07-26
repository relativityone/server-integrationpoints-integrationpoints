using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;

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

            _command = new Command<IConfiguration>(_configuration, _executionConstrains.Object, _executor.Object, new EmptyLogger());
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
            _executor.Setup(x => x.ExecuteAsync(_configuration, CompositeCancellationToken.None)).ReturnsAsync(ExecutionResult.Success());

            // ACT
            ExecutionResult result = await _command.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

            // ASSERT
            _executor.Verify(x => x.ExecuteAsync(_configuration, CompositeCancellationToken.None));
            Assert.AreEqual(result.Status, ExecutionStatus.Completed);
        }

        [Test]
        public void ItShouldThrowOnException()
        {
            _executor.Setup(x => x.ExecuteAsync(_configuration, CompositeCancellationToken.None)).Throws(new InvalidOperationException("Foo bar baz"));

            // ACT
            Func<Task> action = () => _command.ExecuteAsync(CompositeCancellationToken.None);

            // ASSERT
            action.Should().Throw<InvalidOperationException>().WithMessage("Foo bar baz");
        }
    }
}