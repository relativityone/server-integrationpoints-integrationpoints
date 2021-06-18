using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Utils;

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

            Mock<IRandom> randomFake = new Mock<IRandom>();
            Mock<ISyncLog> syncLogMock = new Mock<ISyncLog>();


			_instance = new ServiceFactoryForAdmin(_servicesMgr.Object, _proxyFactory.Object,
                randomFake.Object, syncLogMock.Object);

            _instance.SecondsBetweenRetries = 0.5;
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
		public void ItShouldCreateServiceFactoryAfterTwoFailures()
		{
            IObjectManager objectManager = Mock.Of<IObjectManager>();
			IObjectManager wrappedObjectManager = Mock.Of<IObjectManager>();
			_proxyFactory.SetupSequence(x => x.WrapKeplerService(objectManager, It.IsAny<Func<Task<IObjectManager>>>()))
                .Throws<Exception>().Throws<Exception>().Returns(wrappedObjectManager);

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