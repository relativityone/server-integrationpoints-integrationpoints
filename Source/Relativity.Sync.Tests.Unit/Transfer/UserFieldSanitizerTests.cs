using System;
using System.Linq;
using System.Runtime.Caching;
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
using Relativity.Sync.Utils;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	[TestFixture]
	internal class UserFieldSanitizerTests
	{
		private Mock<IMemoryCache> _memoryCacheStub;
		private Mock<IUserInfoManager> _userInfoManagerMock;

		private UserFieldSanitizer _sut;
		
		private const int _WORKSPACE_ID = 1014023;

		private const int _EXISTING_USER_ARTIFACT_ID = 9;
		private const string _EXISTING_USER_EMAIL = "relativity.admin@kcura.com";

		[SetUp]
		public void SetUp()
		{
			_userInfoManagerMock = new Mock<IUserInfoManager>();

			
			_userInfoManagerMock.Setup(m => m.RetrieveUsersBy(It.IsAny<int>(),
					It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(new UserInfoQueryResultSet { ResultCount = 0, DataResults = Enumerable.Empty<UserInfo>() });

			SetupWorkspaceInfoManagerForUser(_WORKSPACE_ID, _EXISTING_USER_ARTIFACT_ID, _EXISTING_USER_EMAIL);

			Mock<ISourceServiceFactoryForAdmin> serviceFactory = new Mock<ISourceServiceFactoryForAdmin>();
			serviceFactory.Setup(x => x.CreateProxyAsync<IUserInfoManager>())
				.ReturnsAsync(_userInfoManagerMock.Object);

			_memoryCacheStub = new Mock<IMemoryCache>();

			_sut = new UserFieldSanitizer(serviceFactory.Object, _memoryCacheStub.Object);
		}

		[Test]
		public void SupportedType_ShouldBeUser()
		{
			// Act
			RelativityDataType supportedType = _sut.SupportedType;

			// Assert
			supportedType.Should().Be(RelativityDataType.User);
		}

		[Test]
		public async Task SanitizeAsync_ShouldReturnNull_WhenInitialValueIsNull()
		{
			// Act
			object sanitizedValue = await _sut.SanitizeAsync(It.IsAny<int>(), 
					It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), null)
				.ConfigureAwait(false);

			// Assert
			sanitizedValue.Should().BeNull();
		}

		[Test]
		public async Task SanitizeAsync_ShouldReturnEmail_WhenUserIsInCache()
		{
			// Arrange
			const int userArtifactIdInCache = 7;
			const string expectedUserEmail = "relativity.admin@kcura.com";

			object initialValue = GetInitialValue(userArtifactIdInCache);

			_memoryCacheStub.Setup(m => m.Get<string>(It.Is<string>(
					x => x.Contains(userArtifactIdInCache.ToString()))))
				.Returns(expectedUserEmail);

			// Act
			object sanitizedValue = await _sut.SanitizeAsync(
					It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<string>(), initialValue)
				.ConfigureAwait(false);

			// Assert
			sanitizedValue.Should().Be(expectedUserEmail);

			_userInfoManagerMock.Verify(
				x => x.RetrieveUsersBy(It.IsAny<int>(), It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()),
				Times.Never);
		}

		[Test]
		public async Task SanitizeAsync_ShouldReturnEmailAndAddToCache_WhenUserExistsInInstance()
		{
			// Arrange
			const int instanceArtifactId = -1;
			const int workspaceArtifactId = 100;
			const int instanceUserArtifactId = 7;
			const string expectedUserEmail = "relativity.admin@kcura.com";

			object initialValue = GetInitialValue(instanceUserArtifactId);

			SetupWorkspaceInfoManagerForUser(instanceArtifactId, instanceUserArtifactId, expectedUserEmail);
			
			// Act
			object sanitizedValue = await _sut.SanitizeAsync(
					workspaceArtifactId, It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<string>(), initialValue)
				.ConfigureAwait(false);

			// Assert
			sanitizedValue.Should().Be(expectedUserEmail);

			_memoryCacheStub.Verify(x => x.Add(It.IsAny<string>(), sanitizedValue, It.IsAny<CacheItemPolicy>()));

			_userInfoManagerMock.Verify(
				x => x.RetrieveUsersBy(instanceArtifactId, It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()),
				Times.Once);

			_userInfoManagerMock.Verify(
				x => x.RetrieveUsersBy(workspaceArtifactId, It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()),
				Times.Never);
		}

		[Test]
		public async Task SanitizeAsync_ShouldReturnEmailAndAddToCache_WhenUserExistsInWorkspace()
		{
			// Arrange
			const int instanceArtifactId = -1;
			const int workspaceArtifactId = 100;
			const int workspaceUserArtifactId = 7;
			const string expectedUserEmail = "relativity.admin@kcura.com";

			object initialValue = GetInitialValue(workspaceUserArtifactId);

			SetupWorkspaceInfoManagerForUser(workspaceArtifactId, workspaceUserArtifactId, expectedUserEmail);

			// Act
			object sanitizedValue = await _sut.SanitizeAsync(
					workspaceArtifactId, It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<string>(), initialValue)
				.ConfigureAwait(false);

			// Assert
			sanitizedValue.Should().Be(expectedUserEmail);

			_memoryCacheStub.Verify(x => x.Add(It.IsAny<string>(), sanitizedValue, It.IsAny<CacheItemPolicy>()));

			_userInfoManagerMock.Verify(
				x => x.RetrieveUsersBy(instanceArtifactId, It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()),
				Times.Once);
			
			_userInfoManagerMock.Verify(
				x => x.RetrieveUsersBy(workspaceArtifactId, It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()),
				Times.Once);
		}

		[Test]
		public async Task SanitizeAsync_ShouldThrowInvalidExportFieldValueException_WhenUserDoesNotExist()
		{
			// Arrange
			const int nonExistingUserArtifactId = 1;

			object initialValue = GetInitialValue(nonExistingUserArtifactId);

			// Act
			Func<Task> sanitizeAsync = () => _sut.SanitizeAsync(It.IsAny<int>(), 
				It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), initialValue);

			// Assert
			await sanitizeAsync.Should().ThrowAsync<InvalidExportFieldValueException>().ConfigureAwait(false);
		}

		private object GetInitialValue(int userArtifactId)
		{
			return JsonHelpers.DeserializeJson($"{{\"ArtifactID\": {userArtifactId}}}");
		}

		private void SetupWorkspaceInfoManagerForUser(int workspaceArtifactId, int userArtifactId, string expectedUserEmail)
		{
			Func<QueryRequest, bool> queryRequestForUser =
				request => request.Condition == $@"('ArtifactID' == {userArtifactId})";

			_userInfoManagerMock.Setup(m => m.RetrieveUsersBy(
					workspaceArtifactId, It.Is<QueryRequest>(query => queryRequestForUser(query)),
					It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(new UserInfoQueryResultSet
				{
					ResultCount = 1,
					DataResults = new[] { new UserInfo { ArtifactID = userArtifactId, Email = expectedUserEmail } }
				});
		}
	}
}
