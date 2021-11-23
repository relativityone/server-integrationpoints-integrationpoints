using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Banzai;
using Banzai.Factories;
using kCura.WinEDDS.Service.Export;
using Moq;
using Relativity.API;
using Relativity.Services;
using Relativity.Services.InstanceSetting;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;
using Relativity.Sync.Nodes;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Transfer;
using Relativity.Telemetry.APM;

namespace Relativity.Sync.Tests.Integration.Helpers
{
	internal static class ContainerHelper
	{
		private const string _INSTANCE_NAME = "This Instance";

		/// <summary>
		///     Given type T : <see cref="IConfiguration" /> and assuming type FooNode : <see cref="SyncNode{T}" /> exists,
		///     returns registered implementation of FooNode.
		/// </summary>
		/// <typeparam name="TConfig">Child interface of <see cref="IConfiguration" /> whose implementing node we want to retrieve</typeparam>
		/// <param name="container">Container from which the node instance should be resolved</param>
		/// <returns>
		///     Resolved instance of <see cref="INode{SyncExecutionContext}" /> which implements SyncNode&lt;TConfiguration
		///     &gt;
		/// </returns>
		public static INode<SyncExecutionContext> ResolveNode<TConfig>(this IContainer container) where TConfig : IConfiguration
		{
			Type syncNodeType = typeof(SyncNode<>).MakeGenericType(typeof(TConfig));
			Type implementingNodeType = GetSyncNodeImplementationTypes().First(x => x.BaseType == syncNodeType);
			var implementingNode = (INode<SyncExecutionContext>)container.Resolve(implementingNodeType);
			return implementingNode;
		}

		/// <summary>
		///     Returns all types FooNode where FooNode : <see cref="SyncNode{T}" />, where T : <see cref="IConfiguration" />.
		/// </summary>
		public static List<Type> GetSyncNodeImplementationTypes()
		{
			return Assembly.GetAssembly(typeof(SyncNode<>))
				.GetTypes()
				.Where(t => t.BaseType?.IsConstructedGenericType ?? false)
				.Where(t => t.BaseType.GetGenericTypeDefinition() == typeof(SyncNode<>))
				.ToList();
		}

		public static FlowComponent<SyncExecutionContext>[] GetSyncNodesFromRegisteredPipeline(IContainer container, Type pipelineType)
		{
			FlowComponent<SyncExecutionContext>[] GetChildTypes(FlowComponent<SyncExecutionContext> flowComponent)
			{
				var childTypes = flowComponent.Children.SelectMany(x => GetChildTypes(x)).ToArray();
				return new[] {flowComponent}.Concat(childTypes).ToArray();
			}

			FlowComponent<SyncExecutionContext> flows = container.ResolveNamed<FlowComponent<SyncExecutionContext>>(pipelineType.Name);

			return GetChildTypes(flows).Where(x => !x.IsFlow && x.Type?.BaseType?.GetGenericTypeDefinition() == typeof(SyncNode<>)).ToArray();
		}

		/// <summary>
		///     Creates a <see cref="ContainerBuilder" /> with all of Relativity Sync's default implementations registered.
		///     This allows users to override any existing implementations.
		/// </summary>
		public static ContainerBuilder CreateInitializedContainerBuilder()
		{
			ContainerBuilder containerBuilder = new ContainerBuilder();
			ContainerFactory containerFactory = new ContainerFactory();
			IRelativityServices relativityServices = CreateMockedRelativityServices();

			SyncJobParameters parameters = FakeHelper.CreateSyncJobParameters();

			containerFactory.RegisterSyncDependencies(containerBuilder, parameters,
				relativityServices, new SyncJobExecutionConfiguration(), new EmptyLogger());

            MockSearchManagerFactory(containerBuilder);

			return containerBuilder;
		}

		public static IContainer CreateContainer(Action<ContainerBuilder> containerBuilderAction = null)
		{
			ContainerBuilder containerBuilder = CreateInitializedContainerBuilder();

			containerBuilderAction?.Invoke(containerBuilder);

			return containerBuilder.Build();
		}

		/// <summary>
		///     Creates Relativity Services with mocked dependencies
		/// </summary>
		public static IRelativityServices CreateMockedRelativityServices()
		{
			IAPM apm = Mock.Of<IAPM>();
			Mock<IInstanceSettingManager> instanceSettingManager = new Mock<IInstanceSettingManager>();
			InstanceSettingQueryResultSet resultSet = new InstanceSettingQueryResultSet
			{
				Success = true
			};
			resultSet.Results.Add(new Result<Services.InstanceSetting.InstanceSetting>()
			{
				Artifact = new Services.InstanceSetting.InstanceSetting()
				{
					Value = _INSTANCE_NAME
				}
			});
			instanceSettingManager.Setup(x => x.QueryAsync(It.IsAny<Services.Query>())).ReturnsAsync(resultSet);

			Mock<ISyncServiceManager> servicesMgr = new Mock<ISyncServiceManager>();
			servicesMgr.Setup(x => x.CreateProxy<IInstanceSettingManager>(It.IsAny<ExecutionIdentity>())).Returns(instanceSettingManager.Object);

			Uri authenticationUri = new Uri("https://localhost", UriKind.RelativeOrAbsolute);

			IHelper helper = Mock.Of<IHelper>();
			return new RelativityServices(apm, servicesMgr.Object, authenticationUri, helper);
		}

        public static void MockSearchManagerFactory(ContainerBuilder containerBuilder)
        {
            Mock<ISearchManager> searchManager = new Mock<ISearchManager>();
            Mock<ISearchManagerFactory> searchManagerFactory = new Mock<ISearchManagerFactory>();
            searchManagerFactory.Setup(x => x.CreateSearchManagerAsync())
                .Returns(Task.FromResult(searchManager.Object));

            containerBuilder.RegisterInstance(searchManagerFactory.Object).As<ISearchManagerFactory>().SingleInstance();
        }
	}
}