﻿using System;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Objects;
using Relativity.Sync.Authentication;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class ServiceFactoryForUserTests
	{
		private ServiceFactoryForUser _instance;

		private Mock<IAuthTokenGenerator> _tokenGenerator;
		private Mock<IDynamicProxyFactory> _dynamicProxyFactory;

		private const int _USER_ID = 139156;

		[SetUp]
		public void SetUp()
		{
			Mock<IUserContextConfiguration> userContextConfiguration = new Mock<IUserContextConfiguration>();
			userContextConfiguration.Setup(x => x.ExecutingUserId).Returns(_USER_ID);

			var servicesMgr = new Mock<IServicesMgr>();
			servicesMgr.Setup(x => x.GetRESTServiceUrl()).Returns(new Uri("", UriKind.Relative));
			servicesMgr.Setup(x => x.GetServicesURL()).Returns(new Uri("", UriKind.Relative));

			_tokenGenerator = new Mock<IAuthTokenGenerator>();
			_tokenGenerator.Setup(x => x.GetAuthTokenAsync(_USER_ID)).ReturnsAsync("token");

			_dynamicProxyFactory = new Mock<IDynamicProxyFactory>();

			_instance = new ServiceFactoryForUser(userContextConfiguration.Object, servicesMgr.Object, _tokenGenerator.Object, _dynamicProxyFactory.Object, new ServiceFactoryFactory());
		}

		[Test]
		[Ignore("DynamicProxy issue REL-310378")]
		public async Task ItShouldWrapKeplerServiceWithProxy()
		{
			IObjectManager wrappedObjectManager = Mock.Of<IObjectManager>();

			_dynamicProxyFactory.Setup(x => x.WrapKeplerService(It.IsAny<IObjectManager>())).Returns(wrappedObjectManager);

			// ACT
			IObjectManager actualObjectManager = await _instance.CreateProxyAsync<IObjectManager>().ConfigureAwait(false);

			// ASSERT
			actualObjectManager.Should().Be(wrappedObjectManager);
			_dynamicProxyFactory.VerifyAll();
		}

		[Test]
		public async Task ItShouldCreateServiceFactoryOnce()
		{
			// ACT
			await _instance.CreateProxyAsync<IObjectManager>().ConfigureAwait(false);
			await _instance.CreateProxyAsync<IObjectManager>().ConfigureAwait(false);

			// ASSERT
			_tokenGenerator.Verify(x => x.GetAuthTokenAsync(_USER_ID), Times.Once);
		}

		[Test]
		public void ItShouldCreateServiceFactoryOnSecondTryIfFirstOneFailed()
		{
			_tokenGenerator.SetupSequence(x => x.GetAuthTokenAsync(_USER_ID)).Throws<Exception>().ReturnsAsync("token");

			// ACT
			Func<Task> firstTry = async () => await _instance.CreateProxyAsync<IObjectManager>().ConfigureAwait(false);
			Func<Task> secondTry = async () => await _instance.CreateProxyAsync<IObjectManager>().ConfigureAwait(false);

			// ASSERT
			firstTry.Should().Throw<Exception>();
			secondTry.Should().NotThrow();
			const int twice = 2;
			_tokenGenerator.Verify(x => x.GetAuthTokenAsync(_USER_ID), Times.Exactly(twice));
		}
	}
}