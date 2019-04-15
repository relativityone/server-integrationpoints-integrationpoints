using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Common;

namespace Relativity.Sync.Tests.Integration
{
	internal abstract class FailingStepsBase<T> where T : IConfiguration
	{
		private List<Type> _executorTypes;
		private ContainerBuilder _containerBuilder;

		[SetUp]
		public void SetUp()
		{
			_executorTypes = new List<Type>();
			_containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.RegisterStubsForPipelineBuilderTests(_containerBuilder, _executorTypes);
			IntegrationTestsContainerBuilder.MockReporting(_containerBuilder);
		}

		[Test]
		public void ItShouldHandleExceptionAndStopExecutionAfterStepExecutionFails()
		{
			IExecutor<T> executor = new FailingExecutorStub<T>();

			_containerBuilder.RegisterInstance(executor).As<IExecutor<T>>();

			ISyncJob syncJob = _containerBuilder.Build().Resolve<ISyncJob>();

			// ACT
			Func<Task> action = async () => await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<SyncException>();

			_executorTypes.Count.Should().Be(ExpectedNumberOfExecutedSteps());

			// should contain steps run in parallel
			AssertExecutedSteps(_executorTypes);
		}

		protected abstract void AssertExecutedSteps(List<Type> executorTypes);

		[Test]
		public void ItShouldHandleExceptionAndStopExecutionAfterStepConstrainsCheckFails()
		{
			IExecutionConstrains<T> executionConstrains = new FailingExecutionConstrainsStub<T>();

			_containerBuilder.RegisterInstance(executionConstrains).As<IExecutionConstrains<T>>();

			ISyncJob syncJob = _containerBuilder.Build().Resolve<ISyncJob>();

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