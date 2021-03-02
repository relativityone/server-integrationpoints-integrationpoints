using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Logging;
using Relativity.Sync.Executors;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.AutomatedWorkflows.Services.Interfaces;
using Relativity.AutomatedWorkflows.Services.Interfaces.DataContracts.Triggers;


namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	internal sealed class AutomatedWorkflowExecutorTests
	{
		private AutomatedWorkflowExecutor _instance;
		private Mock<IDestinationServiceFactoryForAdmin> _serviceFactory;
		private Mock<IAutomatedWorkflowsService> _triggerProcessorService;
		private Mock<IAutomatedWorkflowTriggerConfiguration> _configuration;

		[SetUp]
		public void SetUp()
		{
			_configuration = new Mock<IAutomatedWorkflowTriggerConfiguration>();
			_configuration.Setup(m => m.SynchronizationExecutionResult).Returns(ExecutionResult.Success());
			_triggerProcessorService = new Mock<IAutomatedWorkflowsService>();
			_serviceFactory = new Mock<IDestinationServiceFactoryForAdmin>();
			_serviceFactory.Setup(m => m.CreateProxyAsync<IAutomatedWorkflowsService>())
				.Returns(Task.FromResult(_triggerProcessorService.Object));
			_instance = new AutomatedWorkflowExecutor(new EmptyLogger(), _serviceFactory.Object);
		}

		[Test]
		public async Task ExecuteAsync_ShouldMakeCallToRawTriggerService()
		{
			await _instance.ExecuteAsync(_configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);
			_triggerProcessorService.Verify(
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
			_triggerProcessorService
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

		[Test]
		public async Task ExecuteAsync_ShouldReturnSuccess_WhenKeplerCanNotReturnAutomatedWorkflowsService()
		{
			_serviceFactory.Setup(m => m.CreateProxyAsync<IAutomatedWorkflowsService>()).Throws<Exception>();
			ExecutionResult result = await _instance.ExecuteAsync(_configuration.Object, CompositeCancellationToken.None)
				.ConfigureAwait(false);
			result.Status.Should().Be(ExecutionStatus.Completed);
		}

		[Test]
		public async Task ExecuteAsync_ShouldReturnSuccess_WhenKeplerReturnsNullAutomatedWorkflowsService()
		{
			_serviceFactory.Setup(m => m.CreateProxyAsync<IAutomatedWorkflowsService>()).Returns(Task.FromResult((IAutomatedWorkflowsService)null));
			ExecutionResult result = await _instance.ExecuteAsync(_configuration.Object, CompositeCancellationToken.None).ConfigureAwait(false);
			result.Status.Should().Be(ExecutionStatus.Completed);
		}
	}
}