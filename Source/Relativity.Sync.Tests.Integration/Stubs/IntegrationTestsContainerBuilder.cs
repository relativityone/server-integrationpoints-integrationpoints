using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Moq;
using Relativity.Sync.Configuration;
using Relativity.Sync.Telemetry;
using Relativity.Telemetry.APM;

namespace Relativity.Sync.Tests.Integration.Stubs
{
	public static class IntegrationTestsContainerBuilder
	{
		public static void RegisterExternalDependenciesAsMocks(ContainerBuilder containerBuilder)
		{
			// Relativity.Telemetry.APM
			Mock<IAPM> apmMock = new Mock<IAPM>();
			Mock<ICounterMeasure> counterMock = new Mock<ICounterMeasure>();
			apmMock.Setup(a => a.CountOperation(It.IsAny<string>(),
				It.IsAny<Guid>(),
				It.IsAny<string>(),
				It.IsAny<string>(),
				It.IsAny<bool>(),
				It.IsAny<int?>(),
				It.IsAny<Dictionary<string, object>>(),
				It.IsAny<IEnumerable<ISink>>())
			).Returns(counterMock.Object);
			containerBuilder.RegisterInstance(apmMock.Object).As<IAPM>();
		}

		public static void MockReporting(ContainerBuilder containerBuilder)
		{
			containerBuilder.RegisterInstance(Mock.Of<ISyncMetrics>()).As<ISyncMetrics>();
			containerBuilder.RegisterInstance(Mock.Of<IProgress<SyncJobState>>()).As<IProgress<SyncJobState>>();
		}

		public static void RegisterStubsForPipelineBuilderTests(ContainerBuilder containerBuilder, List<Type> executorTypes)
		{
			foreach (Type type in GetAllConfigurationInterfaces())
			{
				containerBuilder.RegisterGenericAs(type, typeof(ExecutionConstrainsStub<>), typeof(IExecutionConstrains<>));
				containerBuilder.RegisterGenericAs(type, typeof(ExecutorCollectionExecutedTypesStub<>), typeof(IExecutor<>));
				containerBuilder.RegisterInstance(new ConfigurationStub()).As(type);
			}

			containerBuilder.RegisterInstance(executorTypes).As<List<Type>>();
		}

		public static void MockAllSteps(ContainerBuilder containerBuilder)
		{
			List<Type> steps = GetAllConfigurationInterfaces();
			MockSteps(containerBuilder, steps);
		}

		public static void MockStepsExcept<T>(ContainerBuilder containerBuilder)
		{
			List<Type> steps = GetAllConfigurationInterfacesExcept<T>();
			MockSteps(containerBuilder, steps);
		}

		private static List<Type> GetAllConfigurationInterfacesExcept<T>()
		{
			return GetAllConfigurationInterfaces().Where(t => t != typeof(T)).ToList();
		}

		private static List<Type> GetAllConfigurationInterfaces()
		{
			return Assembly.GetAssembly(typeof(IConfiguration))
				.GetTypes()
				.Where(t => t.IsInterface && t.IsAssignableTo<IConfiguration>() && t != typeof(IConfiguration))
				.ToList();
		}

		private static void MockSteps(ContainerBuilder containerBuilder, IEnumerable<Type> types)
		{
			foreach (Type type in types)
			{
				containerBuilder.RegisterGenericAs(type, typeof(ExecutionConstrainsStub<>), typeof(IExecutionConstrains<>));
				containerBuilder.RegisterGenericAs(type, typeof(ExecutorStub<>), typeof(IExecutor<>));
				containerBuilder.RegisterInstance(new ConfigurationStub()).As(type);
			}
		}

		private static void RegisterGenericAs(this ContainerBuilder builder, Type t, Type concreteGeneric, Type interfaceGeneric)
		{
			Type concrete = concreteGeneric.MakeGenericType(t);
			Type intrface = interfaceGeneric.MakeGenericType(t);
			builder.RegisterType(concrete).As(intrface);
		}
	}
}