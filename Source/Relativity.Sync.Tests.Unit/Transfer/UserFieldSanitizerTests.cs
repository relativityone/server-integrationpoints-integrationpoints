using System;
using System.Linq;
using System.Threading.Tasks;
using Relativity.Sync.Transfer;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.KeplerFactory;
using Relativity.Services.Interfaces.UserInfo;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Interfaces.UserInfo.Models;
using Moq;
using NUnit.Framework;
using FluentAssertions;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	[TestFixture]
	[Parallelizable(ParallelScope.All)]
	internal class UserFieldSanitizerTests
	{
		private Mock<ISourceServiceFactoryForAdmin> _serviceFactoryStub;
		private Mock<IUserInfoManager> _userInfoManagerMock;

		private const int _WORKSPACE_ID = 1014023;
		private const string _ITEM_IDENTIFIER_SOURCE_FIELD_NAME = "Control Number";
		private const string _ITEM_IDENTIFIER = "RND000000";
		private const string _SANITIZING_SOURCE_FIELD_NAME = "Relativity Sync Test User";

		private const int _EXISTING_USER_ARTIFACT_ID = 9;
		private const int _NON_EXISTING_USER_ARTIFACT_ID = 10;
		private const string _EXISTING_USER_EMAIL = "relativity.admin@kcura.com";

		[SetUp]
		public void SetUp()
		{
			_userInfoManagerMock = new Mock<IUserInfoManager>();
			_userInfoManagerMock.Setup(m => m.RetrieveUsersBy(
				It.Is<int>(workspaceId => workspaceId == _WORKSPACE_ID),
				It.Is<QueryRequest>(query => query.Condition == $@"('ArtifactID' == {_EXISTING_USER_ARTIFACT_ID})"),
				It.Is<int>(start => start == 0),
				It.Is<int>(length => length == 1)
			)).ReturnsAsync(new UserInfoQueryResultSet()
			{
				ResultCount = 1,
				DataResults = new [] { new UserInfo { ArtifactID = _EXISTING_USER_ARTIFACT_ID, Email = _EXISTING_USER_EMAIL }  }
			});
			_userInfoManagerMock.Setup(m => m.RetrieveUsersBy(
				It.Is<int>(workspaceId => workspaceId == _WORKSPACE_ID),
				It.Is<QueryRequest>(query => query.Condition == $@"('ArtifactID' == {_NON_EXISTING_USER_ARTIFACT_ID})"),
				It.Is<int>(start => start == 0),
				It.Is<int>(length => length == 1)
			)).ReturnsAsync(new UserInfoQueryResultSet()
			{
				ResultCount = 0,
				DataResults = Enumerable.Empty<UserInfo>()
			});

			_serviceFactoryStub = new Mock<ISourceServiceFactoryForAdmin>();
			_serviceFactoryStub.Setup(x => x.CreateProxyAsync<IUserInfoManager>())
				.ReturnsAsync(_userInfoManagerMock.Object);
		}

		[Test]
		public void SupportedType_ShouldBeUser()
		{
			// Arrange
			var sut = new UserFieldSanitizer(_serviceFactoryStub.Object);

			// Act
			RelativityDataType supportedType = sut.SupportedType;

			// Assert
			supportedType.Should().Be(RelativityDataType.User);
		}

		[Test]
		public async Task SanitizeAsync_ShouldReturnNull_WhenInitialValueIsNull()
		{
			// Arrange
			var sut = new UserFieldSanitizer(_serviceFactoryStub.Object);

			// Act
			object sanitizedValue = await sut.SanitizeAsync(
				_WORKSPACE_ID,
				_ITEM_IDENTIFIER_SOURCE_FIELD_NAME,
				_ITEM_IDENTIFIER,
				_SANITIZING_SOURCE_FIELD_NAME,
				null
				).ConfigureAwait(false);

			// Assert
			sanitizedValue.Should().BeNull();
		}

		[Test]
		public async Task SanitizeAsync_ShouldReturnEmail_WhenUserExists()
		{
			// Arrange
			var sut = new UserFieldSanitizer(_serviceFactoryStub.Object);

			// Act
			object sanitizedValue = await sut.SanitizeAsync(
				_WORKSPACE_ID,
				_ITEM_IDENTIFIER_SOURCE_FIELD_NAME,
				_ITEM_IDENTIFIER,
				_SANITIZING_SOURCE_FIELD_NAME,
				JsonHelpers.DeserializeJson($"{{\"ArtifactID\": {_EXISTING_USER_ARTIFACT_ID}}}")
			).ConfigureAwait(false);

			// Assert
			sanitizedValue.Should().Be(_EXISTING_USER_EMAIL);
		}

		[Test]
		public async Task SanitizeAsync_ShouldThrowInvalidExportFieldValueException_WhenUserDoesNotExists()
		{
			// Arrange
			var sut = new UserFieldSanitizer(_serviceFactoryStub.Object);

			// Act
			Func<Task> sanitizeAsync = () => sut.SanitizeAsync(
				_WORKSPACE_ID,
				_ITEM_IDENTIFIER_SOURCE_FIELD_NAME,
				_ITEM_IDENTIFIER,
				_SANITIZING_SOURCE_FIELD_NAME,
				JsonHelpers.DeserializeJson($"{{\"ArtifactID\": {_NON_EXISTING_USER_ARTIFACT_ID}}}")
			);

			// Assert
			await sanitizeAsync.Should().ThrowAsync<InvalidExportFieldValueException>().ConfigureAwait(false);
		}
	}
}
