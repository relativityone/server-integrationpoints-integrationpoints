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
	internal sealed class NotificationStepTests
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
		public void ItShouldHandleExceptionAndStopExecutionAfterStepConstrainsCheckFails()
		{
			IExecutionConstrains<INotificationConfiguration> executionConstrains = new FailingExecutionConstrainsStub<INotificationConfiguration>();

			_containerBuilder.RegisterInstance(executionConstrains).As<IExecutionConstrains<INotificationConfiguration>>();

			ISyncJob syncJob = _containerBuilder.Build().Resolve<ISyncJob>();

			// ACT
			Func<Task> action = async () => await syncJob.ExecuteAsync(CancellationToken.None).ConfigureAwait(false);

			// ASSERT
			action.Should().Throw<SyncException>();

			const int expectedNumberOfSteps = 13;
			_executorTypes.Count.Should().Be(expectedNumberOfSteps);
			_executorTypes.Should().NotContain(x => x == typeof(INotificationConfiguration));
		}
	}
}