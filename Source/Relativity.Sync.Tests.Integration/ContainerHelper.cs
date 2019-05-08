using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Autofac;
using Banzai;
using Moq;
using Relativity.API;
using Relativity.Sync.Configuration;
using Relativity.Sync.Logging;
using Relativity.Sync.Nodes;
using Relativity.Telemetry.APM;

namespace Relativity.Sync.Tests.Integration
{
	internal static class ContainerHelper
	{
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
			var implementingNode = (INode<SyncExecutionContext>) container.Resolve(implementingNodeType);
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

		/// <summary>
		///     Creates a <see cref="ContainerBuilder" /> with all of Relativity Sync's default implementations registered.
		///     This allows users to override any existing implementations.
		/// </summary>
		public static ContainerBuilder CreateInitializedContainerBuilder()
		{
			ContainerBuilder containerBuilder = new ContainerBuilder();
			ContainerFactory containerFactory = new ContainerFactory();
			RelativityServices relativityServices = CreateMockedRelativityServices();
			containerFactory.RegisterSyncDependencies(containerBuilder, new SyncJobParameters(1, 1), relativityServices, new SyncJobExecutionConfiguration(), new EmptyLogger());
			return containerBuilder;
		}

		/// <summary>
		///     Creates Relativity Services with mocked dependencies
		/// </summary>
		public static RelativityServices CreateMockedRelativityServices()
		{
			IAPM apm = Mock.Of<IAPM>();
			IServicesMgr servicesMgr = Mock.Of<IServicesMgr>();
			Uri authenticationUri = new Uri("https://localhost", UriKind.RelativeOrAbsolute);
			return new RelativityServices(apm, servicesMgr, authenticationUri);
		}
	}
}