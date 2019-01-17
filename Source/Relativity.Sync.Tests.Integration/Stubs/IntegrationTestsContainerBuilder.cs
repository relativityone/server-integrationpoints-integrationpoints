using System;
using System.Collections.Generic;
using Autofac;

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
			return CreateContainerBuilder(executorTypes).Build();
		}

		public static ContainerBuilder CreateContainerBuilder(List<Type> executorTypes)
		{
			ContainerBuilder containerBuilder = new ContainerBuilder();

			containerBuilder.RegisterInstance(new ConfigurationStub()).AsImplementedInterfaces();
			containerBuilder.RegisterGeneric(typeof(ExecutionConstrainsStub<>)).As(typeof(IExecutionConstrains<>));
			containerBuilder.RegisterGeneric(typeof(ExecutorStub<>)).As(typeof(IExecutor<>));
			containerBuilder.RegisterInstance(executorTypes).As<List<Type>>();

			return containerBuilder;
		}
	}
}