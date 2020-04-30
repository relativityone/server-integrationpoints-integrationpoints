using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.AutomatedWorkflows.Services.Interfaces;
using Relativity.AutomatedWorkflows.Services.Interfaces.DataContracts.Triggers;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Executors
{
	[TestFixture]
	internal sealed class AutomatedWorkflowExecutorTests
	{
		private AutomatedWorkflowExecutor _instance;
		private Mock<ISyncServiceManager> _servicesMgr;
		private Mock<IAutomatedWorkflowsService> _triggerProcessorService;
		private Mock<IAutomatedWorkflowTriggerConfiguration> _configuration;

		[SetUp]
		public void SetUp()
		{
			_configuration = new Mock<IAutomatedWorkflowTriggerConfiguration>();
			_configuration.Setup(m => m.SynchronizationExecutionResult).Returns(ExecutionResult.Success());
			_triggerProcessorService = new Mock<IAutomatedWorkflowsService>();
			_servicesMgr = new Mock<ISyncServiceManager>();
			_servicesMgr.Setup(m => m.CreateProxy<IAutomatedWorkflowsService>(ExecutionIdentity.System))
				.Returns(_triggerProcessorService.Object);
			_instance = new AutomatedWorkflowExecutor(new EmptyLogger(), _servicesMgr.Object);
		}

		[Test]
		public async Task ExecuteAsync_ShouldMakeCallToRawTriggerService()
		{
			await _instance.ExecuteAsync(_configuration.Object, CancellationToken.None).ConfigureAwait(false);
			_triggerProcessorService.Verify(
				m => m.SendTriggerAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SendTriggerBody>()), Times.Once);
		}

		[Test]
		public async Task ExecuteAsync_ShouldReturnCompleted_WhenTriggerServiceExecutesWithoutExceptions()
		{
			ExecutionResult result = await _instance.ExecuteAsync(_configuration.Object, CancellationToken.None).ConfigureAwait(false);
			result.Status.Should().Be(ExecutionStatus.Completed);
		}

		[Test]
		public async Task ExecuteAsync_ShouldReturnSuccess_WhenTriggerServiceThrowsException()
		{
			_triggerProcessorService
				.Setup(m => m.SendTriggerAsync(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<SendTriggerBody>()))
				.Throws<Exception>();
			ExecutionResult result = await _instance.ExecuteAsync(_configuration.Object, CancellationToken.None).ConfigureAwait(false);
			result.Status.Should().Be(ExecutionStatus.Completed);
		}

		[Test]
		public async Task ExecuteAsync_ShouldReturnSuccess_WhenSynchronisationResultIsNull()
		{
			_configuration.Setup(m => m.SynchronizationExecutionResult).Returns((ExecutionResult)null);
			ExecutionResult result = await _instance.ExecuteAsync(_configuration.Object, CancellationToken.None).ConfigureAwait(false);
			result.Status.Should().Be(ExecutionStatus.Completed);
		}

		[Test]
		public async Task ExecuteAsync_ShouldReturnSuccess_WhenKeplerCanNotReturnAutomatedWorkflowsService()
		{
			_servicesMgr.Setup(m => m.CreateProxy<IAutomatedWorkflowsService>(ExecutionIdentity.System)).Throws<Exception>();
			ExecutionResult result = await _instance.ExecuteAsync(_configuration.Object, CancellationToken.None)
				.ConfigureAwait(false);
			result.Status.Should().Be(ExecutionStatus.Completed);
		}

		[Test]
		public async Task ExecuteAsync_ShouldReturnSuccess_WhenKeplerReturnsNullAutomatedWorkflowsService()
		{
			_servicesMgr.Setup(m => m.CreateProxy<IAutomatedWorkflowsService>(ExecutionIdentity.System)).Returns((IAutomatedWorkflowsService)null); 
			ExecutionResult result = await _instance.ExecuteAsync(_configuration.Object, CancellationToken.None).ConfigureAwait(false);
			result.Status.Should().Be(ExecutionStatus.Completed);
		}
	}
}