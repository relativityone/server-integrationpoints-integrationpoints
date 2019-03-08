using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Security;
using Relativity.Services.Security.Models;
using Relativity.Sync.Authentication;

namespace Relativity.Sync.Tests.Unit.Authentication
{
	[TestFixture]
	public class OAuth2ClientFactoryTests
	{
		private Mock<IAPILog> _log;
		private Mock<IServicesMgr> _servicesMgr;
		private OAuth2ClientFactory _sut;

		private const string _OAUTH2_CLIENT_NAME_PREFIX = "F6B8C2B4B3E8465CA00775F699375D3C";

		[OneTimeSetUp]
		public void SetUp()
		{
			_log = new Mock<IAPILog>();
			_servicesMgr = new Mock<IServicesMgr>();
			_sut = new OAuth2ClientFactory(_servicesMgr.Object, _log.Object);
		}

		[Test]
		public async Task ItShouldReturnExistingAuthClient()
		{
			const int userId = 1;
			string clientName = $"{_OAUTH2_CLIENT_NAME_PREFIX} {userId}";
			Services.Security.Models.OAuth2Client expectedClient = new Services.Security.Models.OAuth2Client() { Name = clientName };

			Mock<IOAuth2ClientManager> clientManager = new Mock<IOAuth2ClientManager>();
			clientManager.Setup(x => x.ReadAllAsync()).ReturnsAsync(new List<Services.Security.Models.OAuth2Client>() {expectedClient});
			_servicesMgr.Setup(x => x.CreateProxy<IOAuth2ClientManager>(ExecutionIdentity.System)).Returns(clientManager.Object);

			// act
			Services.Security.Models.OAuth2Client actualClient = await _sut.GetOauth2ClientAsync(userId).ConfigureAwait(false);

			// assert
			actualClient.Should().BeEquivalentTo(expectedClient);
			clientManager.Verify(x => x.CreateAsync(It.IsAny<string>(), It.IsAny<OAuth2Flow>(), It.IsAny<IEnumerable<Uri>>(), It.IsAny<int>()),
				Times.Never);
		}

		[Test]
		public async Task ItShouldCreateNewAuthClient()
		{
			const int userId = 1;
			string clientName = $"{_OAUTH2_CLIENT_NAME_PREFIX} {userId}";
			Services.Security.Models.OAuth2Client expectedClient = new Services.Security.Models.OAuth2Client() { Name = clientName };

			Mock<IOAuth2ClientManager> clientManager = new Mock<IOAuth2ClientManager>();
			clientManager.Setup(x => x.ReadAllAsync()).ReturnsAsync(Enumerable.Empty<Services.Security.Models.OAuth2Client>().ToList());
			clientManager.Setup(x => x.CreateAsync(clientName, OAuth2Flow.ClientCredentials, It.IsAny<IEnumerable<Uri>>(), userId)).ReturnsAsync(expectedClient);
			_servicesMgr.Setup(x => x.CreateProxy<IOAuth2ClientManager>(ExecutionIdentity.System)).Returns(clientManager.Object);

			// act
			Services.Security.Models.OAuth2Client actualClient = await _sut.GetOauth2ClientAsync(userId).ConfigureAwait(false);

			// assert
			actualClient.Should().BeEquivalentTo(expectedClient);
			clientManager.Verify(x => x.CreateAsync(clientName, OAuth2Flow.ClientCredentials, It.IsAny<IEnumerable<Uri>>(), userId),
				Times.Once);
		}
	}
}