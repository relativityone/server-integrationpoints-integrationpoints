using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Integration.Stubs;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public sealed class DestinationWorkspaceTagsCreationStepTests
	{
		private SyncJobFactory _syncJobFactory;
		private List<Type> _executorTypes;
		private ContainerBuilder _containerBuilder;

		[SetUp]
		public void SetUp()
		{
			_executorTypes = new List<Type>();
			_syncJobFactory = new SyncJobFactory();
			_containerBuilder = IntegrationTestsContainerBuilder.CreateContainerBuilder(_executorTypes);
		}

		[Test]
		public void ItShouldHandleExceptionAndStopExecutionAfterStepExecutionFails()
		{
			IExecutor<IDestinationWorkspaceTagsCreationConfiguration> executor = new FailingExecutorStub<IDestinationWorkspaceTagsCreationConfiguration>();

			_containerBuilder.RegisterInstance(executor).As<IExecutor<IDestinationWorkspaceTagsCreationConfiguration>>();

			ISyncJob syncJob = _syncJobFactory.Create(_containerBuilder.Build(), new SyncJobParameters(1, 1));

			// ACT
			Func<Task> action = async () => await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<SyncException>();
			// validation, permissions, cleanup, storage init, object types, snapshot, source workspace tags, data destination init, notification
			const int expectedNumberOfExecutedSteps = 9;
			_executorTypes.Count.Should().Be(expectedNumberOfExecutedSteps);

			// should contain steps run in parallel
			_executorTypes.Should().Contain(x => x == typeof(ISourceWorkspaceTagsCreationConfiguration));
			_executorTypes.Should().Contain(x => x == typeof(IDataDestinationInitializationConfiguration));
		}

		[Test]
		public void ItShouldHandleExceptionAndStopExecutionAfterStepConstrainsCheckFails()
		{
			IExecutionConstrains<IDestinationWorkspaceTagsCreationConfiguration> executionConstrains =
				new FailingExecutionConstrainsStub<IDestinationWorkspaceTagsCreationConfiguration>();

			_containerBuilder.RegisterInstance(executionConstrains).As<IExecutionConstrains<IDestinationWorkspaceTagsCreationConfiguration>>();

			ISyncJob syncJob = _syncJobFactory.Create(_containerBuilder.Build(), new SyncJobParameters(1, 1));

			// ACT
			Func<Task> action = async () => await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<SyncException>();
			// validation, permissions, cleanup, storage init, object types, snapshot, source workspace tags, data destination init, notification
			const int expectedNumberOfExecutedSteps = 9;
			_executorTypes.Count.Should().Be(expectedNumberOfExecutedSteps);
			_executorTypes.Should().NotContain(x => x == typeof(IDestinationWorkspaceTagsCreationConfiguration));
		}
	}
}