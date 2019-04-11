using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using Banzai;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Nodes;
using Relativity.Sync.Telemetry;
using Relativity.Sync.Tests.Integration.Stubs;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public sealed class PipelineBuilderTests
	{
		private List<Type> _executorTypes;
		private ContainerBuilder _containerBuilder;

		[SetUp]
		public void SetUp()
		{
			_executorTypes = new List<Type>();

			_containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();

			IntegrationTestsContainerBuilder.MockReporting(_containerBuilder);
			IntegrationTestsContainerBuilder.RegisterStubsForPipelineBuilderTests(_containerBuilder, _executorTypes);
		}

		[Test]
		public async Task PipelineStepsShouldBeInOrder()
		{
			List<Type[]> expectedOrder = ExpectedExecutionOrder();

			ISyncJob syncJob = _containerBuilder.Build().Resolve<ISyncJob>();

			// ACT
			await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			AssertExecutionOrder(expectedOrder);
		}

		[Test]
		public async Task NotificationShouldBeExecutedInCaseOfSuccessfulPipeline()
		{
			ISyncJob syncJob = _containerBuilder.Build().Resolve<ISyncJob>();

			// ACT
			await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_executorTypes.Should().Contain(typeof(INotificationConfiguration));
		}

		[Test]
		public void NotificationShouldBeExecutedInCaseOfFailedPipeline()
		{
			_containerBuilder.RegisterType<FailingExecutorStub<IValidationConfiguration>>().As<IExecutor<IValidationConfiguration>>();

			ISyncJob syncJob = _containerBuilder.Build().Resolve<ISyncJob>();

			// ACT
			Func<Task> action = async () => await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<SyncException>();
			_executorTypes.Should().Contain(typeof(INotificationConfiguration));
		}

		[Test]
		public async Task MetricShouldBeReportedWhileRunningPipeline()
		{
			Mock<ISyncMetrics> syncMetrics = new Mock<ISyncMetrics>();
			_containerBuilder.RegisterInstance(syncMetrics.Object).As<ISyncMetrics>();
			ISyncJob syncJob = _containerBuilder.Build().Resolve<ISyncJob>();

			// ACT
			await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			syncMetrics.Verify(x => x.TimedOperation(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<ExecutionStatus>()));
		}

		[Test]
		public async Task ProgressShouldBeCalledOnEachStep()
		{
			Mock<IProgress<SyncJobState>> progress = new Mock<IProgress<SyncJobState>>();
			_containerBuilder.RegisterInstance(progress.Object).As<IProgress<SyncJobState>>();
			ISyncJob syncJob = _containerBuilder.Build().Resolve<ISyncJob>();

			// ACT
			await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			// We're expecting at least two invocations per node; there will be more for notification steps, etc.
			List<Type> nodes = ContainerHelper.GetSyncNodeImplementationTypes();
			const int two = 2;
			progress.Verify(x => x.Report(It.IsAny<SyncJobState>()), Times.AtLeast(nodes.Count * two));
		}

		[Test]
		public async Task ItShouldCreateCompletedProgressStates()
		{
			Mock<IProgress<SyncJobState>> progress = new Mock<IProgress<SyncJobState>>();
			_containerBuilder.RegisterInstance(progress.Object).As<IProgress<SyncJobState>>();
			IContainer container = _containerBuilder.Build();
			ISyncJob syncJob = container.Resolve<ISyncJob>();

			// ACT
			await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			INode<SyncExecutionContext>[] allSyncNodeImplementations = ContainerHelper.GetSyncNodeImplementationTypes()
				.Select(x => (INode<SyncExecutionContext>)container.Resolve(x)).ToArray();
			AssertNodesReportedCompleted(progress, allSyncNodeImplementations);
		}

		[Test]
		public void ItShouldCreateFailedProgressState()
		{
			Mock<IProgress<SyncJobState>> progress = new Mock<IProgress<SyncJobState>>();
			_containerBuilder.RegisterInstance(progress.Object).As<IProgress<SyncJobState>>();
			IntegrationTestsContainerBuilder.MockFailingStep<IDataSourceSnapshotConfiguration>(_containerBuilder);
			IContainer container = _containerBuilder.Build();
			ISyncJob syncJob = container.Resolve<ISyncJob>();

			// ACT
			Action action = () => syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false).GetAwaiter().GetResult();

			// ASSERT
			action.Should().Throw<SyncException>();
			AssertNodesReportedCompleted(progress,
				container.ResolveNode<IPermissionsCheckConfiguration>(),
				container.ResolveNode<IValidationConfiguration>(),
				container.ResolveNode<IDestinationWorkspaceObjectTypesCreationConfiguration>());
			AssertNodesReportedFailed(progress, container.ResolveNode<IDataSourceSnapshotConfiguration>());
		}

		[Test]
		public async Task ItShouldCreateCompletedWithErrorsProgressState()
		{
			Mock<IProgress<SyncJobState>> progress = new Mock<IProgress<SyncJobState>>();
			_containerBuilder.RegisterInstance(progress.Object).As<IProgress<SyncJobState>>();
			IntegrationTestsContainerBuilder.MockCompletedWithErrorsStep<IDataSourceSnapshotConfiguration>(_containerBuilder);
			IContainer container = _containerBuilder.Build();
			ISyncJob syncJob = container.Resolve<ISyncJob>();

			// ACT
			await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			INode<SyncExecutionContext>[] completedNodes = ContainerHelper.GetSyncNodeImplementationTypes()
				.Where(x => x.BaseType != typeof(SyncNode<IDataSourceSnapshotConfiguration>))
				.Select(x => (INode<SyncExecutionContext>)container.Resolve(x))
				.ToArray();
			AssertNodesReportedCompleted(progress, completedNodes);
			AssertNodesReportedCompletedWithErrors(progress, container.ResolveNode<IDataSourceSnapshotConfiguration>());
		}

		private void AssertExecutionOrder(List<Type[]> expectedOrder)
		{
			int counter = 0;
			foreach (Type[] types in expectedOrder)
			{
				foreach (var type in types)
				{
					bool isInOrder = false;
					for (int j = 0; j < types.Length; j++)
					{
						isInOrder |= type == _executorTypes[j + counter];
					}

					isInOrder.Should().BeTrue();
				}

				counter += types.Length;
			}
		}

		private static List<Type[]> ExpectedExecutionOrder()
		{
			return new List<Type[]>
			{
				new[] {typeof(IPermissionsCheckConfiguration)},
				new[] {typeof(IValidationConfiguration)},
				new[] {typeof(IDestinationWorkspaceObjectTypesCreationConfiguration)},
				new[] {typeof(IDataSourceSnapshotConfiguration)},
				new[]
				{
					typeof(ISourceWorkspaceTagsCreationConfiguration),
					typeof(IDestinationWorkspaceTagsCreationConfiguration),
					typeof(IDataDestinationInitializationConfiguration)
				},
				new[] {typeof(IDestinationWorkspaceSavedSearchCreationConfiguration)},
				new[] {typeof(ISnapshotPartitionConfiguration)},
				new[] {typeof(ISynchronizationConfiguration)},
				new[] {typeof(IDataDestinationFinalizationConfiguration)},
				new[] {typeof(IJobStatusConsolidationConfiguration)},
				new[] {typeof(IJobCleanupConfiguration)},
				new[] {typeof(INotificationConfiguration)}
			};
		}

		private static void AssertNodesReportedCompleted(Mock<IProgress<SyncJobState>> progress, params INode<SyncExecutionContext>[] nodes)
		{
			foreach (INode<SyncExecutionContext> node in nodes)
			{
				SyncJobState expected = SyncJobState.Completed(node.Id);
				progress.Verify(x => x.Report(It.Is<SyncJobState>(v => Equals(expected, v))));
			}
		}

		private static void AssertNodesReportedFailed(Mock<IProgress<SyncJobState>> progress, params INode<SyncExecutionContext>[] nodes)
		{
			foreach (INode<SyncExecutionContext> node in nodes)
			{
				SyncJobState expected = SyncJobState.Failure(node.Id, new InvalidOperationException());
				progress.Verify(x => x.Report(It.Is<SyncJobState>(v => Equals(expected, v))));
			}
		}

		private static void AssertNodesReportedCompletedWithErrors(Mock<IProgress<SyncJobState>> progress, params INode<SyncExecutionContext>[] nodes)
		{
			foreach (INode<SyncExecutionContext> node in nodes)
			{
				SyncJobState expected = SyncJobState.CompletedWithErrors(node.Id);
				progress.Verify(x => x.Report(It.Is<SyncJobState>(v => Equals(expected, v))));
			}
		}

		private static bool Equals(SyncJobState me, SyncJobState you)
		{
			return string.Equals(me.Id, you.Id, StringComparison.InvariantCulture) &&
			       string.Equals(me.Status, you.Status, StringComparison.InvariantCulture) &&
			       string.Equals(me.Message, you.Message, StringComparison.InvariantCulture) &&
			       me.Exception?.GetType() == you.Exception?.GetType() &&
			       string.Equals(me.Exception?.Message, you.Exception?.Message, StringComparison.InvariantCulture);
		}
	}
}