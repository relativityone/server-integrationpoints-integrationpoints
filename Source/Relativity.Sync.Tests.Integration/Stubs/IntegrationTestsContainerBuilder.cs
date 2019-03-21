using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Moq;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.InstanceSetting;
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

		public static void RegisterStubsForPipelineBuilderTests(ContainerBuilder containerBuilder, List<Type> executorTypes)
		{
			RegisterStubsForIntegrationTests(containerBuilder);

			// We can't register these as generics, because the concrete IExecutor<T> registrations override the generic ones for stubs.
			// Therefore, we have to construct the IExecutor<TStub> from IExecutor<> and TStub, and then register that. Ugh.
			GetAllConfigurationInterfaces().ForEach(t => containerBuilder.RegisterGenericAs(t, typeof(ExecutorCollectionExecutedTypesStub<>), typeof(IExecutor<>)));
			containerBuilder.RegisterInstance(executorTypes).As<List<Type>>();
		}

		public static void RegisterStubsForSyncFactoryTests(ContainerBuilder containerBuilder)
		{
			RegisterStubsForIntegrationTests(containerBuilder);

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

			// Relativity.API
			Mock<IHelper> helperMock = new Mock<IHelper>();
			Mock<IDBContext> dbContextMock = new Mock<IDBContext>();
			Mock<IServicesMgr> servicesMgrMock = new Mock<IServicesMgr>();
			helperMock.Setup(h => h.GetDBContext(It.IsAny<int>())).Returns(dbContextMock.Object);
			containerBuilder.RegisterInstance(helperMock.Object).As<IHelper>();
			containerBuilder.RegisterInstance(servicesMgrMock.Object).As<IServicesMgr>();

			// Relativity.Services.InstanceSettings
			Mock<IInstanceSettingManager> instanceSettingsManagerMock = new Mock<IInstanceSettingManager>();
			servicesMgrMock.Setup(x => x.CreateProxy<IInstanceSettingManager>(It.IsAny<ExecutionIdentity>()))
				.Returns(instanceSettingsManagerMock.Object);
			var instanceSettingQueryResultSet = new InstanceSettingQueryResultSet
			{
				Success = true,
				Results = new List<Result<Services.InstanceSetting.InstanceSetting>>()
			};
			instanceSettingsManagerMock.Setup(x => x.QueryAsync(It.IsAny<Services.Query>()))
				.Returns(Task.FromResult(instanceSettingQueryResultSet));
		}

		public static void RegisterStubsForIntegrationTests(ContainerBuilder containerBuilder)
		{
			containerBuilder.RegisterInstance(new ConfigurationStub()).AsImplementedInterfaces();

			// We can't register these as generics, because the concrete IExecutor<T> registrations override the generic ones for stubs.
			// Therefore, we have to construct the IExecutor<TStub> from IExecutor<> and TStub, and then register that. Ugh.
			GetAllConfigurationInterfaces().ForEach(t =>
			{
				containerBuilder.RegisterGenericAs(t, typeof(ExecutionConstrainsStub<>), typeof(IExecutionConstrains<>));
				containerBuilder.RegisterGenericAs(t, typeof(ExecutorStub<>), typeof(IExecutor<>));
			});

			containerBuilder.RegisterType<SyncMetricsStub>().As<ISyncMetrics>();
			containerBuilder.RegisterType<APMClientStub>().As<IAPMClient>();
			containerBuilder.RegisterType<StopwatchStub>().As<IStopwatch>();
			containerBuilder.RegisterInstance(Mock.Of<IServicesMgr>()).As<IServicesMgr>();
			containerBuilder.RegisterInstance(Mock.Of<IProvideServiceUris>()).As<IProvideServiceUris>();
		}

		public static void MockStepsExcept<T>(ContainerBuilder containerBuilder)
		{
			GetAllConfigurationInterfacesExcept<T>().ForEach(t =>
			{
				containerBuilder.RegisterGenericAs(t, typeof(ExecutionConstrainsStub<>), typeof(IExecutionConstrains<>));
				containerBuilder.RegisterGenericAs(t, typeof(ExecutorStub<>), typeof(IExecutor<>));
			});
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

		private static void RegisterGenericAs(this ContainerBuilder builder, Type t, Type concreteGeneric, Type interfaceGeneric)
		{
			Type concrete = concreteGeneric.MakeGenericType(t);
			Type intrface = interfaceGeneric.MakeGenericType(t);
			builder.RegisterType(concrete).As(intrface);
		}
	}
}