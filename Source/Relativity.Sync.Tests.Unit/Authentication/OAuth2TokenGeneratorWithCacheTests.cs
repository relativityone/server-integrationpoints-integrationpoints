using Moq;
using NUnit.Framework;
using Relativity.Sync.Authentication;

namespace Relativity.Sync.Tests.Unit.Authentication
{
	[TestFixture]
	internal sealed class OAuth2TokenGeneratorWithCacheTests
	{
		private Mock<IAuthTokenGenerator> _authTokenGenerator;
		private OAuth2TokenGeneratorWithCache _sut;

		[OneTimeSetUp]
		public void SetUp()
		{
			_authTokenGenerator = new Mock<IAuthTokenGenerator>();
			_authTokenGenerator.Setup(x => x.GetAuthToken(It.IsAny<int>())).Returns("fake_token");
			_sut = new OAuth2TokenGeneratorWithCache(_authTokenGenerator.Object);
		}

		[Test]
		public void ItShouldMakeRequestOnlyOnce()
		{
			const int userId = 1;

			// act
			_sut.GetAuthToken(userId);
			_sut.GetAuthToken(userId);

			// assert
			_authTokenGenerator.Verify(x => x.GetAuthToken(userId), Times.Once);
		}

		[Test]
		public void ItShouldMakeRequestOncePerUserId()
		{
			const int userId1 = 1;
			const int userId2 = 2;

			// act
			_sut.GetAuthToken(userId1);
			_sut.GetAuthToken(userId1);
			_sut.GetAuthToken(userId2);
			_sut.GetAuthToken(userId2);

			// assert
			_authTokenGenerator.Verify(x => x.GetAuthToken(userId1), Times.Once);
			_authTokenGenerator.Verify(x => x.GetAuthToken(userId2), Times.Once);
		}
	}
}