using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Banzai;
using Banzai.Factories;
using FluentAssertions;
using FluentAssertions.Common;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Nodes;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Telemetry.Metrics;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;

namespace Relativity.Sync.Tests.Integration
{
    [TestFixture]
    public sealed class PipelineBuilderTests
    {
        private readonly Mock<IPipelineSelector> _pipelineSelector = new Mock<IPipelineSelector>();

        private List<Type> _executorTypesInActualExecutionOrder;
        private ContainerBuilder _containerBuilder;

        public static IEnumerable<TestCaseData> PipelineTypes()
        {
            var types = typeof(ISyncPipeline).Assembly.GetTypes()
                .Where(x => x.Implements(typeof(ISyncPipeline)) && !x.IsAbstract).ToArray();

            return types.Select(x => new TestCaseData(x));
        }

        [SetUp]
        public void SetUp()
        {
            _executorTypesInActualExecutionOrder = new List<Type>();

            _containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
            _containerBuilder.RegisterInstance(_pipelineSelector.Object).As<IPipelineSelector>();

            IntegrationTestsContainerBuilder.MockReportingWithProgress(_containerBuilder);
            IntegrationTestsContainerBuilder.RegisterStubsForPipelineBuilderTests(_containerBuilder, _executorTypesInActualExecutionOrder);
        }

        [TestCaseSource(nameof(PipelineTypes))]
        public async Task PipelineStepsShouldBeInOrder(Type pipelineType)
        {
            // ARRANGE
            _pipelineSelector.Setup(x => x.GetPipeline()).Returns(() => Activator.CreateInstance(pipelineType) as ISyncPipeline);
            List<Type[]> expectedOrder = PipelinesNodeHelper.GetExpectedNodesInExecutionOrder(pipelineType);

            ISyncJob syncJob = _containerBuilder.Build().Resolve<ISyncJob>();

            // ACT
            await syncJob.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

            // ASSERT
            AssertExecutionOrder(expectedOrder, _executorTypesInActualExecutionOrder);
            expectedOrder.SelectMany(x => x).Count().Should()
                .Be(_executorTypesInActualExecutionOrder.Count,
                    "Expected nodes should have the same count as actual list");
        }

        [TestCaseSource(nameof(PipelineTypes))]
        public async Task NotificationShouldBeExecutedInCaseOfSuccessfulPipeline(Type pipelineType)
        {
            // ARRANGE
            _pipelineSelector.Setup(x => x.GetPipeline()).Returns(() => Activator.CreateInstance(pipelineType) as ISyncPipeline);

            ISyncJob syncJob = _containerBuilder.Build().Resolve<ISyncJob>();

            // ACT
            await syncJob.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

            // ASSERT
            _executorTypesInActualExecutionOrder.Should().Contain(typeof(INotificationConfiguration));
        }

        [TestCaseSource(nameof(PipelineTypes))]
        public void NotificationShouldBeExecutedInCaseOfFailedPipeline(Type pipelineType)
        {
            // ARRANGE
            _pipelineSelector.Setup(x => x.GetPipeline()).Returns(() => Activator.CreateInstance(pipelineType) as ISyncPipeline);

            _containerBuilder.RegisterType<FailingExecutorStub<IValidationConfiguration>>().As<IExecutor<IValidationConfiguration>>();

            ISyncJob syncJob = _containerBuilder.Build().Resolve<ISyncJob>();

            // ACT
            Func<Task> action = () => syncJob.ExecuteAsync(CompositeCancellationToken.None);

            // ASSERT
            action.Should().Throw<SyncException>();
            _executorTypesInActualExecutionOrder.Should().Contain(typeof(INotificationConfiguration));
        }

        [TestCaseSource(nameof(PipelineTypes))]
        public async Task MetricShouldBeReportedWhileRunningPipeline(Type pipelineType)
        {
            // ARRANGE
            _pipelineSelector.Setup(x => x.GetPipeline()).Returns(() => Activator.CreateInstance(pipelineType) as ISyncPipeline);

            Mock<ISyncMetrics> syncMetrics = new Mock<ISyncMetrics>();
            _containerBuilder.RegisterInstance(syncMetrics.Object).As<ISyncMetrics>();
            ISyncJob syncJob = _containerBuilder.Build().Resolve<ISyncJob>();

            // ACT
            await syncJob.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

            // ASSERT
            syncMetrics.Verify(x => x.Send(It.Is<CommandMetric>(m => m.Duration != null)));
        }

        [TestCaseSource(nameof(PipelineTypes))]
        public async Task ProgressShouldBeCalledOnEachStep(Type pipelineType)
        {
            // ARRANGE
            Mock<IProgress<SyncJobState>> progress = new Mock<IProgress<SyncJobState>>();
            _pipelineSelector.Setup(x => x.GetPipeline()).Returns(() => Activator.CreateInstance(pipelineType) as ISyncPipeline);

            _containerBuilder.RegisterInstance(progress.Object).As<IProgress<SyncJobState>>();

            var container = _containerBuilder.Build();
            ISyncJob syncJob = container.Resolve<ISyncJob>();

            // ACT
            await syncJob.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

            // ASSERT
            // We're expecting at least two invocations per node; there will be more for notification steps, etc.
            var flowComponents = ContainerHelper.GetSyncNodesFromRegisteredPipeline(container, pipelineType);

            const int two = 2;
            progress.Verify(x => x.Report(It.IsAny<SyncJobState>()), Times.AtLeast(flowComponents.Length * two));
        }

        [TestCaseSource(nameof(PipelineTypes))]
        public async Task ItShouldCreateCompletedProgressStates(Type pipelineType)
        {
            // ARRANGE
            Mock<IProgress<SyncJobState>> progress = new Mock<IProgress<SyncJobState>>();

            _pipelineSelector.Setup(x => x.GetPipeline()).Returns(() => Activator.CreateInstance(pipelineType) as ISyncPipeline);

            _containerBuilder.RegisterInstance(progress.Object).As<IProgress<SyncJobState>>();
            _containerBuilder.RegisterInstance(_pipelineSelector.Object).As<IPipelineSelector>();

            var container = _containerBuilder.Build();

            ISyncJob syncJob = container.Resolve<ISyncJob>();

            // ACT
            await syncJob.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

            var flowComponents = ContainerHelper.GetSyncNodesFromRegisteredPipeline(container, pipelineType);

            var nodes = flowComponents
                .Select(x => container.Resolve(x.Type) as INode<SyncExecutionContext>).ToArray();

            // ASSERT
            AssertNodesReportedCompleted(progress, nodes);
        }

        [TestCaseSource(nameof(PipelineTypes))]
        public void ItShouldCreateFailedProgressState(Type pipelineType)
        {
            // ARRANGE
            Mock<IProgress<SyncJobState>> progress = new Mock<IProgress<SyncJobState>>();
            _pipelineSelector.Setup(x => x.GetPipeline()).Returns(() => Activator.CreateInstance(pipelineType) as ISyncPipeline);

            _containerBuilder.RegisterInstance(progress.Object).As<IProgress<SyncJobState>>();

            Type dataSnapshotConfigurationType = GetSnapshotNodeType(pipelineType).BaseType.GenericTypeArguments.First();
            IntegrationTestsContainerBuilder.MockFailingStep(dataSnapshotConfigurationType, _containerBuilder);

            IContainer container = _containerBuilder.Build();
            ISyncJob syncJob = container.Resolve<ISyncJob>();

            // ACT
            Action action = () => syncJob.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

            // ASSERT
            action.Should().Throw<SyncException>();
            AssertNodesReportedCompleted(progress,
                container.ResolveNode<IPermissionsCheckConfiguration>(),
                container.ResolveNode<IValidationConfiguration>(),
                container.ResolveNode<IDestinationWorkspaceObjectTypesCreationConfiguration>());

            AssertNodesReportedFailed(progress, container.Resolve(GetSnapshotNodeType(pipelineType)) as INode<SyncExecutionContext>);
        }

        [TestCaseSource(nameof(PipelineTypes))]
        public async Task ItShouldCreateCompletedWithErrorsProgressState(Type pipelineType)
        {
            // ARRANGE
            Mock<IProgress<SyncJobState>> progress = new Mock<IProgress<SyncJobState>>();
            _pipelineSelector.Setup(x => x.GetPipeline()).Returns(() => Activator.CreateInstance(pipelineType) as ISyncPipeline);

            _containerBuilder.RegisterInstance(progress.Object).As<IProgress<SyncJobState>>();

            Type dataSnapshotConfigurationType = GetSnapshotNodeType(pipelineType).BaseType.GenericTypeArguments.First();

            IntegrationTestsContainerBuilder.MockCompletedWithErrorsStep(dataSnapshotConfigurationType, _containerBuilder);

            IContainer container = _containerBuilder.Build();
            ISyncJob syncJob = container.Resolve<ISyncJob>();

            // ACT
            await syncJob.ExecuteAsync(CompositeCancellationToken.None).ConfigureAwait(false);

            // ASSERT
            INode<SyncExecutionContext>[] completedNodes = ContainerHelper.GetSyncNodesFromRegisteredPipeline(container, pipelineType)
                .Where(x => x.Type != GetSnapshotNodeType(pipelineType))
                .Select(x => container.Resolve(x.Type) as INode<SyncExecutionContext>).ToArray();
            AssertNodesReportedCompleted(progress, completedNodes);
            AssertNodesReportedCompletedWithErrors(progress,
                container.Resolve(GetSnapshotNodeType(pipelineType)) as INode<SyncExecutionContext>);
        }

        private static void AssertExecutionOrder(IReadOnlyCollection<Type[]> expectedExecutionOrder, IReadOnlyCollection<Type> actualExecutionOrder)
        {
            int offset = 0;
            foreach (Type[] expectedSteps in expectedExecutionOrder)
            {
                IEnumerable<Type> actualStepsTypesSet = actualExecutionOrder
                    .Skip(offset).Take(expectedSteps.Length);

                actualStepsTypesSet.Should().BeEquivalentTo(expectedSteps);

                offset += expectedSteps.Length;
            }
        }

        private static Type GetSnapshotNodeType(Type pipelineType)
        {
            if (pipelineType == typeof(SyncDocumentRetryPipeline) || pipelineType == typeof(SyncImageRetryPipeline))
            {
                return typeof(DataSourceSnapshotNode);
            }

            if (pipelineType == typeof(SyncDocumentRunPipeline) || pipelineType == typeof(SyncImageRunPipeline) || pipelineType == typeof(IAPI2_SyncDocumentRunPipeline))
            {
                return typeof(DataSourceSnapshotNode);
            }

            if (pipelineType == typeof(SyncNonDocumentRunPipeline))
            {
                return typeof(NonDocumentObjectDataSourceSnapshotNode);
            }

            throw new ArgumentException($"Pipeline {pipelineType.Name} not handled in tests");
        }

        private static void AssertNodesReportedCompleted(Mock<IProgress<SyncJobState>> progress, params INode<SyncExecutionContext>[] nodes)
        {
            foreach (INode<SyncExecutionContext> node in nodes)
            {
                SyncJobState expected = SyncJobState.Completed(node.Id, string.Empty);
                progress.Verify(x => x.Report(It.Is<SyncJobState>(v => Equals(expected, v))));
            }
        }

        private static void AssertNodesReportedCompleted(Mock<IProgress<SyncJobState>> progress, params FlowComponent<SyncExecutionContext>[] nodes)
        {
            foreach (FlowComponent<SyncExecutionContext> node in nodes)
            {
                SyncJobState expected = SyncJobState.Completed(node.Id, string.Empty);
                progress.Verify(x => x.Report(It.Is<SyncJobState>(v => Equals(expected, v))));
            }
        }

        private static void AssertNodesReportedFailed(Mock<IProgress<SyncJobState>> progress, params INode<SyncExecutionContext>[] nodes)
        {
            foreach (INode<SyncExecutionContext> node in nodes)
            {
                SyncJobState expected = SyncJobState.Failure(node.Id, string.Empty, new InvalidOperationException());
                progress.Verify(x => x.Report(It.Is<SyncJobState>(v => Equals(expected, v))));
            }
        }

        private static void AssertNodesReportedCompletedWithErrors(Mock<IProgress<SyncJobState>> progress, params INode<SyncExecutionContext>[] nodes)
        {
            foreach (INode<SyncExecutionContext> node in nodes)
            {
                SyncJobState expected = SyncJobState.CompletedWithErrors(node.Id, string.Empty);
                progress.Verify(x => x.Report(It.Is<SyncJobState>(v => Equals(expected, v))));
            }
        }

        private static bool Equals(SyncJobState me, SyncJobState you)
        {
            return string.Equals(me.Id, you.Id, StringComparison.InvariantCulture) &&
                me.Status == you.Status &&
                string.Equals(me.Message, you.Message, StringComparison.InvariantCulture) &&
                me.Exception?.GetType() == you.Exception?.GetType() &&
                string.Equals(me.Exception?.Message, you.Exception?.Message, StringComparison.InvariantCulture);
        }
    }
}
