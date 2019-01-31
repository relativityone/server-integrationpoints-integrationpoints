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
	internal abstract class FailingStepsBase<T> where T : IConfiguration
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
			IExecutor<T> executor = new FailingExecutorStub<T>();

			_containerBuilder.RegisterInstance(executor).As<IExecutor<T>>();

			ISyncJob syncJob = _syncJobFactory.Create(_containerBuilder.Build(), new SyncJobParameters(1, 1));

			// ACT
			Func<Task> action = async () => await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<SyncException>();

			_executorTypes.Count.Should().Be(ExpectedNumberOfExecutedSteps());

			// should contain steps run in parallel
			//_executorTypes.Should().Contain(x => x == typeof(IDestinationWorkspaceTagsCreationConfiguration));
			//_executorTypes.Should().Contain(x => x == typeof(IDataDestinationInitializationConfiguration));
		}

		protected abstract void AssertExecutedSteps(List<Type> executorTypes);

		[Test]
		public void ItShouldHandleExceptionAndStopExecutionAfterStepConstrainsCheckFails()
		{
			IExecutionConstrains<T> executionConstrains =
				new FailingExecutionConstrainsStub<T>();

			_containerBuilder.RegisterInstance(executionConstrains).As<IExecutionConstrains<T>>();

			ISyncJob syncJob = _syncJobFactory.Create(_containerBuilder.Build(), new SyncJobParameters(1, 1));

			// ACT
			Func<Task> action = async () => await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<SyncException>();

			_executorTypes.Count.Should().Be(ExpectedNumberOfExecutedSteps());
			_executorTypes.Should().NotContain(x => x == typeof(T));
		}

		protected abstract int ExpectedNumberOfExecutedSteps();
	}
}