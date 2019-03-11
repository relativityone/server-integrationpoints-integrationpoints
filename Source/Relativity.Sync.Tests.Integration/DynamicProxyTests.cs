using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Telemetry;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public sealed class DynamicProxyTests
	{
		private IContainer _container;
		private Mock<IServicesMgr> _servicesMgr;
		private Mock<IDynamicProxyFactory> _dynamicProxyFactory;

		[SetUp]
		public void SetUp()
		{
			_servicesMgr = new Mock<IServicesMgr>();
			_dynamicProxyFactory = new Mock<IDynamicProxyFactory>();

			ContainerBuilder containerBuilder = new ContainerBuilder();
			containerBuilder.RegisterInstance(_servicesMgr.Object).As<IServicesMgr>();

			KeplerInstaller keplerInstaller = new KeplerInstaller();
			keplerInstaller.Install(containerBuilder);

			TelemetryInstaller telemetryInstaller = new TelemetryInstaller();
			telemetryInstaller.Install(containerBuilder);

			containerBuilder.RegisterInstance(_dynamicProxyFactory.Object).As<IDynamicProxyFactory>();

			_container = containerBuilder.Build();
		}

		[Test]
		public async Task ItShouldWrapKeplerServiceForUser()
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
	}
}