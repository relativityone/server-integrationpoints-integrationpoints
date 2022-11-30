using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Interfaces.ObjectType;
using Relativity.Services.Interfaces.ObjectType.Models;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.ExecutionConstrains;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.ExecutionConstrains
{
    [TestFixture]
    public sealed class AutomatedWorkflowExecutorConstrainsTests
    {
        private const int _DESTINATION_WORKSPACE_ARTIFACT_ID = 101001;
        private const string _RELATIVITY_APPLICATION_CONDITION = "'Name' == 'Relativity Application'";
        private const int _RELATIVITY_APPLICATIONS_ARTIFACT_ID = 103883;
        private const int _RELATIVITY_APPLICATIONS_ARTIFACT_TYPE_ID = 1000011;
        private const string _AUTOMATED_WORKFLOWS_APPLICATION_CONDITION = "'Name' == 'Automated Workflows'";

        private static IAutomatedWorkflowTriggerConfiguration PrepareAutomatedWorkflowTriggerConfiguration(ExecutionResult executionResult, ArtifactType rdoArtifactType = ArtifactType.Document)
        {
            Mock<IAutomatedWorkflowTriggerConfiguration> configurationFake = new Mock<IAutomatedWorkflowTriggerConfiguration>();
            configurationFake.SetupGet(p => p.SynchronizationExecutionResult).Returns(executionResult);
            configurationFake.SetupGet(p => p.DestinationWorkspaceArtifactId).Returns(_DESTINATION_WORKSPACE_ARTIFACT_ID);
            configurationFake.SetupGet(p => p.RdoArtifactTypeId).Returns((int)rdoArtifactType);

            return configurationFake.Object;
        }

        private static AutomatedWorkflowExecutorConstrains PrepareAutomatedWorkflowExecutorConstrains(bool isAutomatedWorkflowsInstalled)
        {
            Mock<IObjectManager> objectManagerFake = new Mock<IObjectManager>();

            objectManagerFake.Setup(m => m.QuerySlimAsync(
                It.Is<int>(workspaceId => workspaceId == _DESTINATION_WORKSPACE_ARTIFACT_ID),
                It.Is<QueryRequest>(qr => qr.ObjectType.ArtifactTypeID == (int)ArtifactType.ObjectType && qr.Condition == _RELATIVITY_APPLICATION_CONDITION),
                It.Is<int>(start => start == 0),
                It.Is<int>(length => length == 1))).Returns(Task.FromResult(new QueryResultSlim
            {
                TotalCount = 1,
                ResultCount = 1,
                Objects = new List<RelativityObjectSlim>
            {
                new RelativityObjectSlim { ArtifactID = _RELATIVITY_APPLICATIONS_ARTIFACT_ID }
            }
            }));

            objectManagerFake.Setup(m => m.QuerySlimAsync(
                It.Is<int>(workspaceId => workspaceId == _DESTINATION_WORKSPACE_ARTIFACT_ID),
                It.Is<QueryRequest>(qr => qr.ObjectType.ArtifactTypeID == _RELATIVITY_APPLICATIONS_ARTIFACT_TYPE_ID && qr.Condition == _AUTOMATED_WORKFLOWS_APPLICATION_CONDITION),
                It.Is<int>(start => start == 0),
                It.Is<int>(length => length == 0))).Returns(Task.FromResult(new QueryResultSlim { TotalCount = isAutomatedWorkflowsInstalled ? 1 : 0 }));

            Mock<IObjectTypeManager> objectTypeManagerFake = new Mock<IObjectTypeManager>();

            objectTypeManagerFake.Setup(m => m.ReadAsync(
                It.Is<int>(workspaceId => workspaceId == _DESTINATION_WORKSPACE_ARTIFACT_ID),
                It.Is<int>(objectTypeId => objectTypeId == _RELATIVITY_APPLICATIONS_ARTIFACT_ID))).Returns(Task.FromResult(new ObjectTypeResponse { ArtifactTypeID = _RELATIVITY_APPLICATIONS_ARTIFACT_TYPE_ID }));

            Mock<IDestinationServiceFactoryForAdmin> serviceFactoryFake = new Mock<IDestinationServiceFactoryForAdmin>();
            serviceFactoryFake.Setup(m => m.CreateProxyAsync<IObjectManager>()).Returns(Task.FromResult(objectManagerFake.Object));
            serviceFactoryFake.Setup(m => m.CreateProxyAsync<IObjectTypeManager>()).Returns(Task.FromResult(objectTypeManagerFake.Object));

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

        [Test]
        public async Task CanExecuteAsync_ShouldBeFalse_WhenAIsNonDocumentObjectFlow()
        {
            // ARRANGE
            IAutomatedWorkflowTriggerConfiguration configuration = PrepareAutomatedWorkflowTriggerConfiguration(ExecutionResult.SuccessWithErrors(new Exception()), ArtifactType.View);

            AutomatedWorkflowExecutorConstrains sut = PrepareAutomatedWorkflowExecutorConstrains(true);

            // ACT
            bool canExecute = await sut.CanExecuteAsync(configuration, CancellationToken.None).ConfigureAwait(false);

            // ASSERT
            canExecute.Should().BeFalse();
        }
    }
}
