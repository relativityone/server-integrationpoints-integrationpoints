﻿using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Services.ServiceProxy;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Tests.Integration.Stubs;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public sealed class DynamicProxyTests
	{
		private IContainer _container;
		private IObjectManager _wrappedObjectManager;

		[SetUp]
		public void SetUp()
		{
			ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.MockAllSteps(containerBuilder);
			IntegrationTestsContainerBuilder.MockMetrics(containerBuilder);

			var servicesMgr = new Mock<IServicesMgr>();
			var serviceFactory = new Mock<IServiceFactory>();
			var dynamicProxyFactory = new Mock<IDynamicProxyFactory>();

			containerBuilder.RegisterInstance(servicesMgr.Object).As<IServicesMgr>();
			containerBuilder.Register(k => new ServiceFactoryForUser(serviceFactory.Object, dynamicProxyFactory.Object)).As<ISourceServiceFactoryForUser>();
			containerBuilder.Register(k => new ServiceFactoryForUser(serviceFactory.Object, dynamicProxyFactory.Object)).As<IDestinationServiceFactoryForUser>();
			containerBuilder.RegisterInstance(dynamicProxyFactory.Object).As<IDynamicProxyFactory>();

			_container = containerBuilder.Build();

			IObjectManager objectManager = Mock.Of<IObjectManager>();
			_wrappedObjectManager = Mock.Of<IObjectManager>();

			servicesMgr.Setup(x => x.CreateProxy<IObjectManager>(ExecutionIdentity.System)).Returns(objectManager);
			serviceFactory.Setup(x => x.CreateProxy<IObjectManager>()).Returns(objectManager);
			dynamicProxyFactory.Setup(x => x.WrapKeplerService(objectManager)).Returns(_wrappedObjectManager);
		}

		[Test]
		public async Task ItShouldWrapSourceKeplerServiceForAdmin()
		{
			ISourceServiceFactoryForAdmin serviceFactory = _container.Resolve<ISourceServiceFactoryForAdmin>();

			// ACT
			IObjectManager actualObjectManager = await serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false);

			// ASSERT
			actualObjectManager.Should().Be(_wrappedObjectManager);
		}

		[Test]
		public async Task ItShouldWrapSourceKeplerServiceForUser()
		{
			ISourceServiceFactoryForUser serviceFactory = _container.Resolve<ISourceServiceFactoryForUser>();

			// ACT
			IObjectManager actualObjectManager = await serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false);

			// ASSERT
			actualObjectManager.Should().Be(_wrappedObjectManager);
		}

		[Test]
		public async Task ItShouldWrapDestinationKeplerServiceForAdmin()
		{
			IDestinationServiceFactoryForAdmin serviceFactory = _container.Resolve<IDestinationServiceFactoryForAdmin>();

			// ACT
			IObjectManager actualObjectManager = await serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false);

			// ASSERT
			actualObjectManager.Should().Be(_wrappedObjectManager);
		}

		[Test]
		public async Task ItShouldWrapDestinationKeplerServiceForUser()
		{
			IDestinationServiceFactoryForUser serviceFactory = _container.Resolve<IDestinationServiceFactoryForUser>();

			// ACT
			IObjectManager actualObjectManager = await serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false);

			// ASSERT
			actualObjectManager.Should().Be(_wrappedObjectManager);
		}
	}
}