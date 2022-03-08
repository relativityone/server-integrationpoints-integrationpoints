using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.OAuth2Client.Interfaces;
using Relativity.Services.Objects;
using Relativity.Services.Security;
using Relativity.Services.ServiceProxy;
using Relativity.Sync.Authentication;
using Relativity.Sync.Configuration;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Tests.Integration.Helpers;
using Relativity.Telemetry.APM;

namespace Relativity.Sync.Tests.Integration
{
	[TestFixture]
	public sealed class UserContextTests
	{
		private ContainerBuilder _containerBuilder;
		private ServiceFactoryFactoryStub _serviceFactoryFactoryStub;

		private const int _USER_ID = 123;
		private const string _CLIENT_NAME = "F6B8C2B4B3E8465CA00775F699375D3C 123";
		private const string _CLIENT_ID = "id";
		private const string _CLIENT_SECRET = "secret";
		private const string _INSTANCE_URL = "https://relativity.one";
		private const string _AUTH_ENDPOINT = "https://relativity.one/Identity/connect/token";
		private const string _REST_URL = "https://relativity.one/rest";
		private const string _TOKEN = "token";


		[SetUp]
		public void SetUp()
		{
			_containerBuilder = ContainerHelper.CreateInitializedContainerBuilder();
			IntegrationTestsContainerBuilder.MockAllSteps(_containerBuilder);
			IntegrationTestsContainerBuilder.MockReportingWithProgress(_containerBuilder);

			_serviceFactoryFactoryStub = new ServiceFactoryFactoryStub();
			_containerBuilder.RegisterInstance(_serviceFactoryFactoryStub).As<IServiceFactoryFactory>();

			MockUserId();
			MockOAuth2ClientManagerToReturnValidClient();
			MockProvideServiceUrisToProvideValidUris();
			MockTokenProviderToProvideGivenToken();
		}

		private void MockUserId()
		{
			Mock<IUserContextConfiguration> userContextConfiguration = new Mock<IUserContextConfiguration>();
			userContextConfiguration.Setup(x => x.ExecutingUserId).Returns(_USER_ID);
			_containerBuilder.RegisterInstance(userContextConfiguration.Object).As<IUserContextConfiguration>();
		}

		private void MockOAuth2ClientManagerToReturnValidClient()
		{
			Mock<IOAuth2ClientManager> oAuth2ClientManager = new Mock<IOAuth2ClientManager>();
			List<Services.Security.Models.OAuth2Client> clients = new List<Services.Security.Models.OAuth2Client>
			{
				new Services.Security.Models.OAuth2Client
				{
					Name = _CLIENT_NAME,
					Id = _CLIENT_ID,
					Secret = _CLIENT_SECRET
				}
			};
			oAuth2ClientManager.Setup(x => x.ReadAllAsync()).ReturnsAsync(clients);

            Mock<ISyncServiceManager> serviceMgr = new Mock<ISyncServiceManager>();
            _containerBuilder.RegisterInstance(serviceMgr.Object).As<ISyncServiceManager>();
            serviceMgr.Setup(x => x.CreateProxy<IOAuth2ClientManager>(ExecutionIdentity.System)).Returns(oAuth2ClientManager.Object);
            serviceMgr.Setup(x => x.GetRESTServiceUrl()).Returns(new Uri(_REST_URL));
		}

		private void MockProvideServiceUrisToProvideValidUris()
		{
			IAPM apm = Mock.Of<IAPM>();
            ISyncServiceManager servicesMgr = Mock.Of<ISyncServiceManager>();
			Uri authenticationUri = new Uri(_INSTANCE_URL);
			IHelper helper = Mock.Of<IHelper>();

			IRelativityServices relativityServices = new RelativityServices(apm, servicesMgr, authenticationUri, helper);
			_containerBuilder.RegisterInstance(relativityServices).As<IRelativityServices>();
		}

		private void MockTokenProviderToProvideGivenToken()
		{
			Mock<ITokenProvider> tokenProvider = new Mock<ITokenProvider>();
			tokenProvider.Setup(x => x.GetAccessTokenAsync()).ReturnsAsync(_TOKEN);

			Mock<ITokenProviderFactory> tokenProviderFactory = new Mock<ITokenProviderFactory>();
			tokenProviderFactory.Setup(x => x.GetTokenProvider("WebApi", new List<string> {"UserInfoAccess"})).Returns(tokenProvider.Object);

			Mock<ITokenProviderFactoryFactory> tokenProviderFactoryFactory = new Mock<ITokenProviderFactoryFactory>();
			tokenProviderFactoryFactory.Setup(x => x.Create(new Uri(_AUTH_ENDPOINT), _CLIENT_ID, _CLIENT_SECRET)).Returns(tokenProviderFactory.Object);
			_containerBuilder.RegisterInstance(tokenProviderFactoryFactory.Object).As<ITokenProviderFactoryFactory>();
		}

		[Test]
        public async Task ItShouldCreateSourceKeplerServiceInUserContext()
		{
			ISourceServiceFactoryForUser factoryForUser = _containerBuilder.Build().Resolve<ISourceServiceFactoryForUser>();

			// ACT
			await factoryForUser.CreateProxyAsync<IObjectManager>().ConfigureAwait(false);

			// ASSERT
			_serviceFactoryFactoryStub.Settings.Should().NotBeNull();
			_serviceFactoryFactoryStub.Settings.RelativityRestUri.Should().Be(_REST_URL);
			_serviceFactoryFactoryStub.Settings.Credentials.Should().BeOfType<BearerTokenCredentials>();
			((BearerTokenCredentials) _serviceFactoryFactoryStub.Settings.Credentials).Token.Should().Be(_TOKEN);
		}
	}
}