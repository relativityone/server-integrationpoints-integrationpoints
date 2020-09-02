using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Tests.Unit.ExecutionConstrains
{
	[TestFixture]
	public sealed class AutomatedWorkflowExecutorConstrainsTests
	{
		private const int _DESTINATION_WORKSPACE_ARTIFACT_ID = 101001;
		private const int _RELATIVITY_APPLICATIONS_ARTIFACT_TYPE_ID = 1000014;
		private const string _AUTOMATED_WORKFLOWS_APPLICATION_CONDITION = "'Name' == 'Automated Workflows'";

		private static IAutomatedWorkflowTriggerConfiguration PrepareAutomatedWorkflowTriggerConfiguration(ExecutionResult executionResult)
		{
			Mock<IAutomatedWorkflowTriggerConfiguration> configurationFake = new Mock<IAutomatedWorkflowTriggerConfiguration>();
			configurationFake.SetupGet(p => p.SynchronizationExecutionResult).Returns(executionResult);
			configurationFake.SetupGet(p => p.DestinationWorkspaceArtifactId).Returns(_DESTINATION_WORKSPACE_ARTIFACT_ID);

			return configurationFake.Object;
		}

		private static AutomatedWorkflowExecutorConstrains PrepareAutomatedWorkflowExecutorConstrains(bool isAutomatedWorkflowsInstalled)
		{
			Mock<IObjectManager> objectManagerFake = new Mock<IObjectManager>();
			objectManagerFake.Setup(m => m.QuerySlimAsync(
				It.Is<int>(workspaceId => workspaceId == _DESTINATION_WORKSPACE_ARTIFACT_ID),
				It.Is<QueryRequest>(qr => qr.ObjectType.ArtifactTypeID == _RELATIVITY_APPLICATIONS_ARTIFACT_TYPE_ID && qr.Condition == _AUTOMATED_WORKFLOWS_APPLICATION_CONDITION),
				It.Is<int>(start => start == 0),
				It.Is<int>(length => length == 0)
			)).Returns(Task.FromResult(new QueryResultSlim { TotalCount = isAutomatedWorkflowsInstalled ? 1 : 0 }));

			Mock<IDestinationServiceFactoryForAdmin> serviceFactoryFake = new Mock<IDestinationServiceFactoryForAdmin>();
			serviceFactoryFake.Setup(m => m.CreateProxyAsync<IObjectManager>()).Returns(Task.FromResult(objectManagerFake.Object));

			return new AutomatedWorkflowExecutorConstrains(serviceFactoryFake.Object, new EmptyLogger());
		}

		[Test]
		public async Task CanExecuteAsync_ShouldBeFalse_WhenSynchronizationExecutionResultIsNull()
		{
			// ARRANGE
			IAutomatedWorkflowTriggerConfiguration configuration = PrepareAutomatedWorkflowTriggerConfiguration(null);

			AutomatedWorkflowExecutorConstrains sut = PrepareAutomatedWorkflowExecutorConstrains(true);

			// ACT
			bool canExecute = await sut.CanExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			canExecute.Should().BeFalse();
		}

		[Test]
		public async Task CanExecuteAsync_ShouldBeFalse_WhenSynchronizationExecutionResultIsCanceled()
		{
			// ARRANGE
			IAutomatedWorkflowTriggerConfiguration configuration = PrepareAutomatedWorkflowTriggerConfiguration(ExecutionResult.Canceled());

			AutomatedWorkflowExecutorConstrains sut = PrepareAutomatedWorkflowExecutorConstrains(true);

			// ACT
			bool canExecute = await sut.CanExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			canExecute.Should().BeFalse();
		}

		[Test]
		public async Task CanExecuteAsync_ShouldBeFalse_WhenSynchronizationExecutionResultIsSkipped()
		{
			// ARRANGE
			IAutomatedWorkflowTriggerConfiguration configuration = PrepareAutomatedWorkflowTriggerConfiguration(ExecutionResult.Skipped());

			AutomatedWorkflowExecutorConstrains sut = PrepareAutomatedWorkflowExecutorConstrains(true);

			// ACT
			bool canExecute = await sut.CanExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			canExecute.Should().BeFalse();
		}

		[Test]
		public async Task CanExecuteAsync_ShouldBeFalse_WhenSynchronizationExecutionResultIsFailure()
		{
			// ARRANGE
			IAutomatedWorkflowTriggerConfiguration configuration = PrepareAutomatedWorkflowTriggerConfiguration(ExecutionResult.Failure(new Exception()));

			AutomatedWorkflowExecutorConstrains sut = PrepareAutomatedWorkflowExecutorConstrains(true);

			// ACT
			bool canExecute = await sut.CanExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			canExecute.Should().BeFalse();
		}

		[Test]
		public async Task CanExecuteAsync_ShouldBeTrue_WhenSynchronizationExecutionResultIsSuccessAndAutomatedWorkflowsIsInstalled()
		{
			// ARRANGE
			IAutomatedWorkflowTriggerConfiguration configuration = PrepareAutomatedWorkflowTriggerConfiguration(ExecutionResult.Success());

			AutomatedWorkflowExecutorConstrains sut = PrepareAutomatedWorkflowExecutorConstrains(true);

			// ACT
			bool canExecute = await sut.CanExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			canExecute.Should().BeTrue();
		}

		[Test]
		public async Task CanExecuteAsync_ShouldBeFalse_WhenSynchronizationExecutionResultIsSuccessAndAutomatedWorkflowsIsNotInstalled()
		{
			// ARRANGE
			IAutomatedWorkflowTriggerConfiguration configuration = PrepareAutomatedWorkflowTriggerConfiguration(ExecutionResult.Success());

			AutomatedWorkflowExecutorConstrains sut = PrepareAutomatedWorkflowExecutorConstrains(false);

			// ACT
			bool canExecute = await sut.CanExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			canExecute.Should().BeFalse();
		}

		[Test]
		public async Task CanExecuteAsync_ShouldBeTrue_WhenSynchronizationExecutionResultIsSuccessWithErrorsAndAutomatedWorkflowsIsInstalled()
		{
			// ARRANGE
			IAutomatedWorkflowTriggerConfiguration configuration = PrepareAutomatedWorkflowTriggerConfiguration(ExecutionResult.SuccessWithErrors(new Exception()));

			AutomatedWorkflowExecutorConstrains sut = PrepareAutomatedWorkflowExecutorConstrains(true);

			// ACT
			bool canExecute = await sut.CanExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			canExecute.Should().BeTrue();
		}

		[Test]
		public async Task CanExecuteAsync_ShouldBeFalse_WhenSynchronizationExecutionResultIsSuccessWithErrorsAndAutomatedWorkflowsIsNotInstalled()
		{
			// ARRANGE
			IAutomatedWorkflowTriggerConfiguration configuration = PrepareAutomatedWorkflowTriggerConfiguration(ExecutionResult.SuccessWithErrors(new Exception()));

			AutomatedWorkflowExecutorConstrains sut = PrepareAutomatedWorkflowExecutorConstrains(false);

			// ACT
			bool canExecute = await sut.CanExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			canExecute.Should().BeFalse();
		}
	}
}
