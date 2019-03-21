using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
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

			IntegrationTestsContainerBuilder.MockMetrics(_containerBuilder);
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
	}
}