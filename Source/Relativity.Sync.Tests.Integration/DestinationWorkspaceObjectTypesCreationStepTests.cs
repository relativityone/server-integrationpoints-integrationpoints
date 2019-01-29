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
	public sealed class DestinationWorkspaceObjectTypesCreationStepTests
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
			IExecutor<IDestinationWorkspaceObjectTypesCreationConfiguration> executor = new FailingExecutorStub<IDestinationWorkspaceObjectTypesCreationConfiguration>();

			_containerBuilder.RegisterInstance(executor).As<IExecutor<IDestinationWorkspaceObjectTypesCreationConfiguration>>();

			ISyncJob syncJob = _syncJobFactory.Create(_containerBuilder.Build(), new SyncJobParameters(1, 1));

			// ACT
			Func<Task> action = async () => await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<SyncException>();
			// validation, permissions, cleanup, storage init, notification
			const int expectedNumberOfExecutedSteps = 5;
			_executorTypes.Count.Should().Be(expectedNumberOfExecutedSteps);
		}

		[Test]
		public void ItShouldHandleExceptionAndStopExecutionAfterStepConstrainsCheckFails()
		{
			IExecutionConstrains<IDestinationWorkspaceObjectTypesCreationConfiguration> executionConstrains =
				new FailingExecutionConstrainsStub<IDestinationWorkspaceObjectTypesCreationConfiguration>();

			_containerBuilder.RegisterInstance(executionConstrains).As<IExecutionConstrains<IDestinationWorkspaceObjectTypesCreationConfiguration>>();

			ISyncJob syncJob = _syncJobFactory.Create(_containerBuilder.Build(), new SyncJobParameters(1, 1));

			// ACT
			Func<Task> action = async () => await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<SyncException>();
			// validation, permissions, cleanup, storage init, notification
			const int expectedNumberOfExecutedSteps = 5;
			_executorTypes.Count.Should().Be(expectedNumberOfExecutedSteps);
			_executorTypes.Should().NotContain(x => x == typeof(IDestinationWorkspaceObjectTypesCreationConfiguration));
		}
	}
}