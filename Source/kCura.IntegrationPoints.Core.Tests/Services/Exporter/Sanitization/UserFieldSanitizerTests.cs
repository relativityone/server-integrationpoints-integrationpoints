using kCura.Apps.Common.Utils.Serializers;
using kCura.IntegrationPoints.Core.Services.Exporter.Sanitization;
using Relativity;
using Relativity.API;
using Relativity.Services.Interfaces.UserInfo;
using Relativity.Services.Interfaces.UserInfo.Models;
using Moq;
using NUnit.Framework;
using FluentAssertions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Relativity.Services.Objects.DataContracts;

namespace kCura.IntegrationPoints.Core.Tests.Services.Exporter.Sanitization
{
	[TestFixture]
	internal class UserFieldSanitizerTests
	{
		private Mock<IHelper> _helperStub;
		private Mock<IUserInfoManager> _userInfoManagerMock;
		private Mock<ISanitizationDeserializer> _sanitizationHelperStub;

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
				It.Is<int>(workspaceId => workspaceId == -1),
				It.Is<QueryRequest>(query => query.Condition == $@"('ArtifactID' == {_EXISTING_USER_ARTIFACT_ID})"),
				It.Is<int>(start => start == 0),
				It.Is<int>(length => length == 1)
			)).ReturnsAsync(new UserInfoQueryResultSet()
			{
				ResultCount = 1,
				DataResults = new[] { new UserInfo { ArtifactID = _EXISTING_USER_ARTIFACT_ID, Email = _EXISTING_USER_EMAIL } }
			});
			_userInfoManagerMock.Setup(m => m.RetrieveUsersBy(
				It.Is<int>(workspaceId => workspaceId == -1),
				It.Is<QueryRequest>(query => query.Condition == $@"('ArtifactID' == {_NON_EXISTING_USER_ARTIFACT_ID})"),
				It.Is<int>(start => start == 0),
				It.Is<int>(length => length == 1)
			)).ReturnsAsync(new UserInfoQueryResultSet()
			{
				ResultCount = 0,
				DataResults = Enumerable.Empty<UserInfo>()
			});

			Mock<IServicesMgr> servicesMgrStub = new Mock<IServicesMgr>();
			servicesMgrStub.Setup(x => x.CreateProxy<IUserInfoManager>(ExecutionIdentity.System))
				.Returns(_userInfoManagerMock.Object);

			_helperStub = new Mock<IHelper>();
			_helperStub.Setup(x => x.GetServicesManager())
				.Returns(servicesMgrStub.Object);

			JSONSerializer jsonSerializer = new JSONSerializer();
			_sanitizationHelperStub = new Mock<ISanitizationDeserializer>();
			_sanitizationHelperStub.Setup(x => x.DeserializeAndValidateExportFieldValue<UserInfo>(
					It.IsAny<string>(),
					It.IsAny<string>(),
					It.IsAny<object>()))
				.Returns((string x, string y, object serializedObject) =>
					jsonSerializer.Deserialize<UserInfo>(serializedObject.ToString()));
		}

		[Test]
		public void SupportedType_ShouldBeUser()
		{
			// Arrange
			var sut = new UserFieldSanitizer(_helperStub.Object, _sanitizationHelperStub.Object);

			// Act
			FieldTypeHelper.FieldType supportedType = sut.SupportedType;

			// Assert
			supportedType.Should().Be(FieldTypeHelper.FieldType.User);
		}

		[Test]
		public async Task SanitizeAsync_ShouldReturnNull_WhenInitialValueIsNull()
		{
			// Arrange
			var sut = new UserFieldSanitizer(_helperStub.Object, _sanitizationHelperStub.Object);

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
			var sut = new UserFieldSanitizer(_helperStub.Object, _sanitizationHelperStub.Object);

			// Act
			object sanitizedValue = await sut.SanitizeAsync(
				_WORKSPACE_ID,
				_ITEM_IDENTIFIER_SOURCE_FIELD_NAME,
				_ITEM_IDENTIFIER,
				_SANITIZING_SOURCE_FIELD_NAME,
				SanitizationTestUtils.DeserializeJson($"{{\"ArtifactID\": {_EXISTING_USER_ARTIFACT_ID}}}")
			).ConfigureAwait(false);

			// Assert
			sanitizedValue.Should().Be(_EXISTING_USER_EMAIL);
		}

		[Test]
		public void SanitizeAsync_ShouldThrowInvalidExportFieldValueException_WhenUserDoesNotExists()
		{
			// Arrange
			var sut = new UserFieldSanitizer(_helperStub.Object, _sanitizationHelperStub.Object);

			// Act
			Action sanitize = () => sut.SanitizeAsync(
				_WORKSPACE_ID,
				_ITEM_IDENTIFIER_SOURCE_FIELD_NAME,
				_ITEM_IDENTIFIER,
				_SANITIZING_SOURCE_FIELD_NAME,
				SanitizationTestUtils.DeserializeJson($"{{\"ArtifactID\": {_NON_EXISTING_USER_ARTIFACT_ID}}}")
			).GetAwaiter().GetResult();

			// Assert
			sanitize.ShouldThrow<InvalidExportFieldValueException>();
		}
	}
}
