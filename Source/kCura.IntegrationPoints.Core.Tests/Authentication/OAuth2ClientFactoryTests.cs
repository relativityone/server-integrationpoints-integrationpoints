using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Authentication;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Security;
using Relativity.Services.Security.Models;

namespace kCura.IntegrationPoints.Core.Tests.Authentication
{
	[TestFixture]
	public class OAuth2ClientFactoryTests : TestBase
	{
		private IHelper _helper;
		private IAPILog _logger;
		private IOAuth2ClientManager _oAuth2ClientManager;
		private int _contextUserId;
		private string _clientName;
		private OAuth2ClientFactory _instance;
		private OAuth2Client _oauth2Client;

		public override void SetUp()
		{
			_contextUserId = 1234;
			_clientName = $"{Constants.IntegrationPoints.OAUTH2_CLIENT_NAME_PREFIX} {_contextUserId}";

			_oAuth2ClientManager = Substitute.For<IOAuth2ClientManager>();
			_logger = Substitute.For<IAPILog>();
			_helper = Substitute.For<IHelper>();
			_helper.GetLoggerFactory().GetLogger().ForContext<OAuth2ClientFactory>().Returns(_logger);
			_helper.GetServicesManager().CreateProxy<IOAuth2ClientManager>(Arg.Any<ExecutionIdentity>())
				.Returns(_oAuth2ClientManager);

			_oauth2Client = new OAuth2Client()
			{
				ContextUser = _contextUserId,
				Name = _clientName
			};

			_instance = new OAuth2ClientFactory(_helper);
		}

		[Test]
		public void ItShouldReturnExistingOAuth2Client()
		{
			_oAuth2ClientManager.ReadAllAsync().Returns(Task.FromResult(new List<OAuth2Client>() {_oauth2Client}));

			OAuth2Client result = _instance.GetOauth2Client(_contextUserId);

			Assert.IsNotNull(result);
			Assert.AreEqual(_clientName, result.Name);
			Assert.AreEqual(_contextUserId, result.ContextUser);
		}

		[Test]
		public void ItShouldCreateNewOAuth2Client()
		{
			_oAuth2ClientManager.ReadAllAsync().Returns(Task.FromResult(new List<OAuth2Client>()));
			_oAuth2ClientManager.CreateAsync(_clientName, OAuth2Flow.ClientCredentials, Arg.Any<IEnumerable<Uri>>(), _contextUserId)
				.Returns(Task.FromResult(_oauth2Client));

			OAuth2Client result = _instance.GetOauth2Client(_contextUserId);

			Assert.IsNotNull(result);
			Assert.AreEqual(_clientName, result.Name);
			Assert.AreEqual(_contextUserId, result.ContextUser);
		}

		[Test]
		public void ItShouldLogErrorWhenRetrievalFails()
		{
			_oAuth2ClientManager.ReadAllAsync().Throws<InvalidOperationException>();
			Assert.Throws<InvalidOperationException>(() => _instance.GetOauth2Client(_contextUserId));
			_logger.Received(1).LogError(Arg.Any<InvalidOperationException>(), Arg.Any<string>());
		}
	}
}