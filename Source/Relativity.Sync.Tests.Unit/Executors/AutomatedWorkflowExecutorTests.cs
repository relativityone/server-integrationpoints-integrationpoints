using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Storage;
using Relativity.Sync.Storage.V2.Models.Triggers;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Executors
{
    [TestFixture]
    internal sealed class AutomatedWorkflowExecutorTests
    {
        private AutomatedWorkflowExecutor _instance;
        private Mock<IAutomatedWorkflowsManager> _automatedWorkflowsManager;
        private Mock<IAutomatedWorkflowTriggerConfiguration> _configuration;

        [SetUp]
        public void SetUp()
        {
            _configuration = new Mock<IAutomatedWorkflowTriggerConfiguration>();
            _configuration.Setup(m => m.SynchronizationExecutionResult).Returns(ExecutionResult.Success());
            _automatedWorkflowsManager = new Mock<IAutomatedWorkflowsManager>();
            _instance = new AutomatedWorkflowExecutor(new EmptyLogger(), _automatedWorkflowsManager.Object);
        }

        [Test]
        public async Task ExecuteAsync_ShouldMakeCallToRawTriggerService()
        {
            await _instance.ExecuteAsync(_configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);
            _automatedWorkflowsManager.Verify(
                m => m.SendTriggerAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SendTriggerBody>()), Times.Once);
        }

        [Test]
        public async Task ExecuteAsync_ShouldReturnCompleted_WhenTriggerServiceExecutesWithoutExceptions()
        {
            ExecutionResult result = await _instance.ExecuteAsync(_configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);
            result.Status.Should().Be(ExecutionStatus.Completed);
        }

        [Test]
        public async Task ExecuteAsync_ShouldReturnSuccess_WhenTriggerServiceThrowsException()
        {
            _automatedWorkflowsManager
                .Setup(m => m.SendTriggerAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SendTriggerBody>()))
                .Throws<Exception>();
            ExecutionResult result = await _instance.ExecuteAsync(_configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);
            result.Status.Should().Be(ExecutionStatus.Completed);
        }

        [Test]
        public async Task ExecuteAsync_ShouldReturnSuccess_WhenSynchronizationResultIsNull()
        {
            _configuration.Setup(m => m.SynchronizationExecutionResult).Returns((ExecutionResult)null);
            ExecutionResult result = await _instance.ExecuteAsync(_configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);
            result.Status.Should().Be(ExecutionStatus.Completed);
        }
    }
}
