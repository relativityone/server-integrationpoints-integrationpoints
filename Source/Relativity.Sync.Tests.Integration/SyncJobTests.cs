using System;
using System.Collections.Generic;
using System.Threading;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Tests.Integration.Stubs;
using Banzai;
using Relativity.Sync.Nodes;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public sealed class SyncJobTests
	{
		private ContainerBuilder _containerBuilder;

		[SetUp]
		public void SetUp()
		{
			_containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.MockAllSteps(_containerBuilder);
			IntegrationTestsContainerBuilder.MockMetrics(_containerBuilder);
		}

		[Test]
		public void ItShouldAggregateExecutionResultExceptions()
		{
			RegisterExecutorMock<IPermissionsCheckConfiguration>( _containerBuilder,
				ExecutionResult.SuccessWithErrors(new ArgumentException("foo")));
			RegisterExecutorMock<IDataDestinationFinalizationConfiguration>(_containerBuilder,
				ExecutionResult.Success());
			RegisterExecutorMock<IJobCleanupConfiguration>(_containerBuilder,
				ExecutionResult.Failure(new InvalidOperationException("bar")));
			SyncJob job = CreateSyncJob(_containerBuilder);

			// Act
			SyncException thrownException = Assert.ThrowsAsync<SyncException>(async () => await job.ExecuteAsync(CancellationToken.None).ConfigureAwait(false));

			// Assert
			AggregateException aggregateException = thrownException.InnerException as AggregateException;
			aggregateException.Should().NotBeNull();

			const int expectedInnerExceptions = 2;
			aggregateException.InnerExceptions.Count.Should().Be(expectedInnerExceptions);
			aggregateException.InnerExceptions[0].Should().BeOfType<ArgumentException>();
			aggregateException.InnerExceptions[0].Message.Should().Be("foo");
			aggregateException.InnerExceptions[1].Should().BeOfType<InvalidOperationException>();
			aggregateException.InnerExceptions[1].Message.Should().Be("bar");
		}

		private static void RegisterExecutorMock<T>(ContainerBuilder containerBuilder, ExecutionResult result) where T : IConfiguration
		{
			Mock<IExecutor<T>> executorMock = new Mock<IExecutor<T>>();
			executorMock.Setup(e => e.ExecuteAsync(It.IsAny<T>(), CancellationToken.None)).ReturnsAsync(result);
			containerBuilder.RegisterInstance(executorMock.Object).As<IExecutor<T>>();
		}

		private static SyncJob CreateSyncJob(ContainerBuilder containerBuilder)
		{
			IContainer container = containerBuilder.Build();
			return new SyncJob(container.Resolve<INode<SyncExecutionContext>>(),
				container.Resolve<ISyncExecutionContextFactory>(),
				new CorrelationId("lksjdf"),
				Mock.Of<ISyncLog>());
		}
	}
}
