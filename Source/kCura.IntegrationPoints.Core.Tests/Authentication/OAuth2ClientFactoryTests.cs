using System;
using System.Collections.Generic;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Common;
using kCura.IntegrationPoints.Core.Authentication;
using kCura.IntegrationPoints.Data;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Security;
using Relativity.Services.Security.Models;

namespace kCura.IntegrationPoints.Core.Tests.Authentication
{
	[TestFixture]
	public class OAuth2ClientFactoryTests : TestBase
	{
		private Mock<IHelper> _helperFake;
		private Mock<IAPILog> _loggerFake;
		private Mock<IOAuth2ClientManager> _oAuth2ClientManagerFake;
		private Mock<IRetryHandlerFactory> _retryHandlerFactoryFake;
		private int _contextUserId;
		private string _clientName;
		private OAuth2ClientFactory _instance;
		private OAuth2Client _oauth2Client;

		public override void SetUp()
		{
			_contextUserId = 1234;
			_clientName = $"{Constants.IntegrationPoints.OAUTH2_CLIENT_NAME_PREFIX} {_contextUserId}";

			_oAuth2ClientManagerFake = new Mock<IOAuth2ClientManager>();
			_loggerFake = new Mock<IAPILog>();
			_helperFake = new Mock<IHelper>();
			_retryHandlerFactoryFake = new Mock<IRetryHandlerFactory>();
			_retryHandlerFactoryFake.Setup(x => x.Create(It.IsAny<ushort>(), It.IsAny<ushort>())).Returns(new RetryHandler(_loggerFake.Object, 1, 1));
			_helperFake.Setup(x => x.GetLoggerFactory().GetLogger().ForContext<OAuth2ClientFactory>()).Returns(_loggerFake.Object);
			_helperFake.Setup(x => x.GetServicesManager().CreateProxy<IOAuth2ClientManager>(It.IsAny<ExecutionIdentity>()))
				.Returns(_oAuth2ClientManagerFake.Object);

			_oauth2Client = new OAuth2Client()
			{
				ContextUser = _contextUserId,
				Name = _clientName
			};

			_instance = new OAuth2ClientFactory(_retryHandlerFactoryFake.Object, _helperFake.Object);
		}

		[Test]
		public void GetOauth2Client_ShouldReturnExistingOAuth2Client()
		{
			// Arrange
			_oAuth2ClientManagerFake.Setup(x => x.ReadAllAsync()).ReturnsAsync(new List<OAuth2Client>() {_oauth2Client});

			// Act
			OAuth2Client result = _instance.GetOauth2Client(_contextUserId);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(_clientName, result.Name);
			Assert.AreEqual(_contextUserId, result.ContextUser);
		}

		[Test]
		public void GetOauth2Client_ShouldCreateNewOAuth2Client()
		{
			// Arrange
			_oAuth2ClientManagerFake.Setup(x => x.ReadAllAsync()).ReturnsAsync(new List<OAuth2Client>());
			_oAuth2ClientManagerFake.Setup(x => x.CreateAsync(_clientName, OAuth2Flow.ClientCredentials, It.IsAny<IEnumerable<Uri>>(), _contextUserId))
				.ReturnsAsync(_oauth2Client);
			
			// Act
			OAuth2Client result = _instance.GetOauth2Client(_contextUserId);

			// Assert
			Assert.IsNotNull(result);
			Assert.AreEqual(_clientName, result.Name);
			Assert.AreEqual(_contextUserId, result.ContextUser);
		}

		[Test]
		public void GetOauth2Client_ShouldLogErrorWhenRetrievalFails()
		{
			// Arrange
			_oAuth2ClientManagerFake.Setup(x => x.ReadAllAsync()).Throws<InvalidOperationException>();

			// Act && Assert
			Assert.Throws<InvalidOperationException>(() => _instance.GetOauth2Client(_contextUserId));
			_loggerFake.Verify(x => x.LogError(It.IsAny<Exception>(), It.IsAny<string>(), It.IsAny<object>()));
		}
	}
}