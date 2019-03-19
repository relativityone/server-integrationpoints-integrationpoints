using System.Threading.Tasks;
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
		private Mock<IServicesMgr> _servicesMgr;
		private Mock<IServiceFactory> _serviceFactory;
		private Mock<IDynamicProxyFactory> _dynamicProxyFactory;

		[SetUp]
		public void SetUp()
		{
			ContainerBuilder containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.RegisterStubsForIntegrationTests(containerBuilder);

			_servicesMgr = new Mock<IServicesMgr>();
			_serviceFactory = new Mock<IServiceFactory>();
			_dynamicProxyFactory = new Mock<IDynamicProxyFactory>();

			containerBuilder.RegisterInstance(_servicesMgr.Object).As<IServicesMgr>();
			containerBuilder.Register(k => new ServiceFactoryForUser(_serviceFactory.Object, _dynamicProxyFactory.Object)).As<ISourceServiceFactoryForUser>();
			containerBuilder.RegisterInstance(_dynamicProxyFactory.Object).As<IDynamicProxyFactory>();

			_container = containerBuilder.Build();
		}

		[Test]
		public async Task ItShouldWrapKeplerServiceForAdmin()
		{
			IObjectManager objectManager = Mock.Of<IObjectManager>();
			IObjectManager wrappedObjectManager = Mock.Of<IObjectManager>();

			_servicesMgr.Setup(x => x.CreateProxy<IObjectManager>(ExecutionIdentity.System)).Returns(objectManager);
			_dynamicProxyFactory.Setup(x => x.WrapKeplerService(objectManager)).Returns(wrappedObjectManager);

			ISourceServiceFactoryForAdmin serviceFactory = _container.Resolve<ISourceServiceFactoryForAdmin>();

			// ACT
			IObjectManager actualObjectManager = await serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false);

			// ASSERT
			actualObjectManager.Should().Be(wrappedObjectManager);
		}

		[Test]
		public async Task ItShouldWrapKeplerServiceForUser()
		{
			IObjectManager objectManager = Mock.Of<IObjectManager>();
			IObjectManager wrappedObjectManager = Mock.Of<IObjectManager>();

			_serviceFactory.Setup(x => x.CreateProxy<IObjectManager>()).Returns(objectManager);
			_dynamicProxyFactory.Setup(x => x.WrapKeplerService(objectManager)).Returns(wrappedObjectManager);

			ISourceServiceFactoryForUser serviceFactory = _container.Resolve<ISourceServiceFactoryForUser>();

			// ACT
			IObjectManager actualObjectManager = await serviceFactory.CreateProxyAsync<IObjectManager>().ConfigureAwait(false);

			// ASSERT
			actualObjectManager.Should().Be(wrappedObjectManager);
		}
	}
}