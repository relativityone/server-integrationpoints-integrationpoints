﻿using System;
using System.Linq;
using System.Threading;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Banzai;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;

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
			IntegrationTestsContainerBuilder.MockReporting(_containerBuilder);
		}

		[Test]
		public void ItShouldAggregateExecutionResultExceptions()
		{
			RegisterExecutorMock<IPermissionsCheckConfiguration>( _containerBuilder,
				ExecutionResult.SuccessWithErrors(new ArgumentException("foo")));
			RegisterExecutorMock<IDataDestinationFinalizationConfiguration>(_containerBuilder,
				ExecutionResult.Success());
			RegisterExecutorMock<IValidationConfiguration>(_containerBuilder,
				ExecutionResult.Failure(new InvalidOperationException("bar")));
			SyncJob job = CreateSyncJob(_containerBuilder);

			// Act
			SyncException thrownException = Assert.ThrowsAsync<SyncException>(async () => await job.ExecuteAsync(CancellationToken.None).ConfigureAwait(false));

			// Assert
			AggregateException aggregateException = thrownException.InnerException as AggregateException;
			aggregateException.Should().NotBeNull();

			const int expectedInnerExceptions = 2;
			aggregateException.InnerExceptions.Count.Should().Be(expectedInnerExceptions);
			aggregateException.InnerExceptions.Any(x => x is ArgumentException && x.Message.Equals("foo", StringComparison.InvariantCulture))
				.Should().BeTrue();
			aggregateException.InnerExceptions.Any(x => x is InvalidOperationException && x.Message.Equals("bar", StringComparison.InvariantCulture))
				.Should().BeTrue();
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
				new SyncJobParameters(1, 1, 1), 
				Mock.Of<IProgress<SyncJobState>>(),
				Mock.Of<ISyncLog>());
		}
	}
}
