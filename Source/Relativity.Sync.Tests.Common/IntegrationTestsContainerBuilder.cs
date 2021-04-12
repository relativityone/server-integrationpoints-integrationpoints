using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using Autofac;
using Moq;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.SumReporting;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Common
{
	[ExcludeFromCodeCoverage]
	public static class IntegrationTestsContainerBuilder
	{
		public static void MockReporting(ContainerBuilder containerBuilder)
		{
			containerBuilder.RegisterInstance(Mock.Of<ISyncMetrics>()).As<ISyncMetrics>();

			var jobEndMetricsServiceFactory = new Mock<IJobEndMetricsServiceFactory>();
			jobEndMetricsServiceFactory.Setup(x => x.CreateJobEndMetricsService(It.IsAny<bool>()))
				.Returns(new EmptyJobEndMetricsService());

			containerBuilder.RegisterInstance(jobEndMetricsServiceFactory.Object).As<IJobEndMetricsServiceFactory>();
		}

		public static void MockReportingWithProgress(ContainerBuilder containerBuilder)
		{
			MockReporting(containerBuilder);

			containerBuilder.RegisterInstance(Mock.Of<IProgress<SyncJobState>>()).As<IProgress<SyncJobState>>();
		}

		public static void RegisterStubsForPipelineBuilderTests(ContainerBuilder containerBuilder, List<Type> executorTypes)
		{
			IEnumerable<Type> configurationInterfaces = GetAllConfigurationInterfaces();
			foreach (Type type in configurationInterfaces)
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

		public static void MockFailingStep<T>(ContainerBuilder containerBuilder)
		{
			containerBuilder.RegisterGenericAs<T>(typeof(ExecutionConstrainsStub<>), typeof(IExecutionConstrains<>));
			containerBuilder.RegisterGenericAs<T>(typeof(FailingExecutorStub<>), typeof(IExecutor<>));
			containerBuilder.RegisterInstance(new ConfigurationStub()).As<T>();
		}
		public static void MockFailingStep(Type stepConfigurationType, ContainerBuilder containerBuilder)
		{
			containerBuilder.RegisterType(typeof(ExecutionConstrainsStub<>).MakeGenericType(stepConfigurationType)).As(typeof(IExecutionConstrains<>).MakeGenericType(stepConfigurationType));
			containerBuilder.RegisterType(typeof(FailingExecutorStub<>).MakeGenericType(stepConfigurationType)).As(typeof(IExecutor<>).MakeGenericType(stepConfigurationType));
			containerBuilder.RegisterInstance(new ConfigurationStub()).As(stepConfigurationType);
		}

		public static void MockCompletedWithErrorsStep<T>(ContainerBuilder containerBuilder)
		{
			containerBuilder.RegisterGenericAs<T>(typeof(ExecutionConstrainsStub<>), typeof(IExecutionConstrains<>));
			containerBuilder.RegisterGenericAs<T>(typeof(CompletedWithErrorsExecutorStub<>), typeof(IExecutor<>));
			containerBuilder.RegisterInstance(new ConfigurationStub()).As<T>();
		}

		public static void MockCompletedWithErrorsStep(Type stepConfigurationType, ContainerBuilder containerBuilder)
		{
			containerBuilder.RegisterType(typeof(ExecutionConstrainsStub<>).MakeGenericType(stepConfigurationType))
				.As(typeof(IExecutionConstrains<>).MakeGenericType(stepConfigurationType));

			containerBuilder.RegisterType(typeof(CompletedWithErrorsExecutorStub<>).MakeGenericType(stepConfigurationType)).As(typeof(IExecutor<>).MakeGenericType(stepConfigurationType));

			containerBuilder.RegisterInstance(new ConfigurationStub()).As(stepConfigurationType);
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

		private static void RegisterGenericAs<T>(this ContainerBuilder builder, Type concreteGeneric, Type interfaceGeneric) => RegisterGenericAs(builder, typeof(T), concreteGeneric, interfaceGeneric);

		private static void RegisterGenericAs(this ContainerBuilder builder, Type t, Type concreteGeneric, Type interfaceGeneric)
		{
			Type concrete = concreteGeneric.MakeGenericType(t);
			Type intrface = interfaceGeneric.MakeGenericType(t);
			builder.RegisterType(concrete).As(intrface);
		}
	}
}