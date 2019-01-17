using System;
using System.Collections.Generic;
using Autofac;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Integration.Stubs
{
	internal static class IntegrationTestsContainerBuilder
	{
		public static IContainer CreateContainer()
		{
			return CreateContainer(new List<Type>());
		}

		public static IContainer CreateContainer(List<Type> executorTypes)
		{
			ContainerBuilder containerBuilder = new ContainerBuilder();

			containerBuilder.RegisterInstance(new ConfigurationStub()).AsImplementedInterfaces();
			containerBuilder.RegisterGeneric(typeof(ExecutionConstrainsStub<>)).As(typeof(IExecutionConstrains<>));
			containerBuilder.RegisterGeneric(typeof(ExecutorStub<>)).As(typeof(IExecutor<>));
			containerBuilder.RegisterInstance(executorTypes).As<List<Type>>();
			containerBuilder.RegisterType<SyncMetricsStub>().As<ISyncMetrics>();

			return containerBuilder.Build();
		}
	}
}