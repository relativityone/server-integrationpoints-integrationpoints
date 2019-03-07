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
		private SyncJobFactory _syncJobFactory;
		private List<Type> _executorTypes;

		[SetUp]
		public void SetUp()
		{
			_executorTypes = new List<Type>();
			_syncJobFactory = new SyncJobFactory();
		}

		[Test]
		public async Task PipelineStepsShouldBeInOrder()
		{
			List<Type[]> expectedOrder = ExpectedExecutionOrder();

			IContainer container = IntegrationTestsContainerBuilder.CreateContainer(_executorTypes);
			ISyncJob syncJob = _syncJobFactory.Create(container, new SyncJobParameters(1, 1));

			// ACT
			await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			AssertExecutionOrder(expectedOrder);
		}

		[Test]
		public async Task NotificationShouldBeExecutedInCaseOfSuccessfulPipeline()
		{
			IContainer container = IntegrationTestsContainerBuilder.CreateContainer(_executorTypes);
			ISyncJob syncJob = _syncJobFactory.Create(container, new SyncJobParameters(1, 1));

			// ACT
			await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			_executorTypes.Should().Contain(typeof(INotificationConfiguration));
		}

		[Test]
		public void NotificationShouldBeExecutedInCaseOfFailedPipeline()
		{
			ContainerBuilder containerBuilder = IntegrationTestsContainerBuilder.CreateContainerBuilder(_executorTypes);

			containerBuilder.RegisterType<FailingExecutorStub<IValidationConfiguration>>().As<IExecutor<IValidationConfiguration>>();

			ISyncJob syncJob = _syncJobFactory.Create(containerBuilder.Build(), new SyncJobParameters(1, 1));

			// ACT
			Func<Task> action = async () => await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<SyncException>();
			_executorTypes.Should().Contain(typeof(INotificationConfiguration));
		}

		[Test]
		public async Task MetricShouldBeReportedWhileRunningPipeline()
		{
			ContainerBuilder containerBuilder = IntegrationTestsContainerBuilder.CreateContainerBuilder(_executorTypes);
			Mock<ISyncMetrics> syncMetrics = new Mock<ISyncMetrics>();
			containerBuilder.RegisterInstance(syncMetrics.Object).As<ISyncMetrics>();
			ISyncJob syncJob = _syncJobFactory.Create(containerBuilder.Build(), new List<IInstaller>(), new SyncJobParameters(1, 1));

			// ACT
			await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			syncMetrics.Verify(x => x.TimedOperation(It.IsAny<string>(), It.IsAny<TimeSpan>(), It.IsAny<CommandExecutionStatus>()));
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