using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class ServiceFactoryForAdminTests
	{
		private ServiceFactoryForAdmin _instance;

		private Mock<ISyncServiceManager> _servicesMgr;
		private Mock<IDynamicProxyFactory> _proxyFactory;

		[SetUp]
		public void SetUp()
		{
			_servicesMgr = new Mock<ISyncServiceManager>();
			_proxyFactory = new Mock<IDynamicProxyFactory>();

			_instance = new ServiceFactoryForAdmin(_servicesMgr.Object, _proxyFactory.Object);
		}

		[Test]
		public async Task ItShouldWrapKeplerServiceWithProxy()
		{
			IObjectManager objectManager = Mock.Of<IObjectManager>();
			IObjectManager wrappedObjectManager = Mock.Of<IObjectManager>();

			_servicesMgr.Setup(x => x.CreateProxy<IObjectManager>(ExecutionIdentity.System)).Returns(objectManager);
			_proxyFactory.Setup(x => x.WrapKeplerService(objectManager, It.IsAny<Func<Task<IObjectManager>>>())).Returns(wrappedObjectManager);

			// ACT
			IObjectManager actualObjectManager = await _instance.CreateProxyAsync<IObjectManager>().ConfigureAwait(false);

			// ASSERT
			actualObjectManager.Should().Be(wrappedObjectManager);
			_servicesMgr.VerifyAll();
			_proxyFactory.VerifyAll();
		}

		[Test]
		public void ItShouldNotThrowWhenCannotResolveService()
		{
			_servicesMgr.Setup(x => x.CreateProxy<IObjectManager>(ExecutionIdentity.System)).Returns((IObjectManager) null);

			// act
			Func<Task> action = async () => await _instance.CreateProxyAsync<IObjectManager>().ConfigureAwait(false);

			// assert
			action.Should().NotThrow();
		}

        [Test]
        public void ItShouldThrowExceptionWhenRetriesLimitReached()
        {
            _servicesMgr.Setup(x => x.CreateProxy<IObjectManager>(ExecutionIdentity.System)).Throws<Exception>();

            // act
            Func<Task> action = async () => await _instance.CreateProxyAsync<IObjectManager>().ConfigureAwait(false);

            // assert
            action.Should().Throw<Exception>();
        }
	}
}