using System;
using System.Linq;
using System.Runtime.Caching;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Interfaces.UserInfo;
using Relativity.Services.Interfaces.UserInfo.Models;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.DbContext;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Tests.Common;
using Relativity.Sync.Toggles;
using Relativity.Sync.Transfer;
using Relativity.Sync.Utils;
using Relativity.Toggles;

namespace Relativity.Sync.Tests.Unit.Transfer
{
	[TestFixture]
	internal class UserFieldSanitizerTests
	{
		private Mock<IMemoryCache> _memoryCacheStub;
		private Mock<IUserInfoManager> _userInfoManagerMock;
		private Mock<IEddsDbContext> _eddsDbContextFake;
		private Mock<IToggleProvider> _toggleProviderFake;

		private UserFieldSanitizer _sut;

		[SetUp]
		public void SetUp()
		{
			_userInfoManagerMock = new Mock<IUserInfoManager>();
			_userInfoManagerMock.Setup(m => m.RetrieveUsersBy(It.IsAny<int>(),
					It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>()))
				.ReturnsAsync(new UserInfoQueryResultSet { ResultCount = 0, DataResults = Enumerable.Empty<UserInfo>() });

			Mock<ISourceServiceFactoryForAdmin> serviceFactoryForAdmin = new Mock<ISourceServiceFactoryForAdmin>();
			serviceFactoryForAdmin.Setup(x => x.CreateProxyAsync<IUserInfoManager>())
				.ReturnsAsync(_userInfoManagerMock.Object);

			Mock<IAPILog> syncLog = new Mock<IAPILog>();

			_memoryCacheStub = new Mock<IMemoryCache>();

			_eddsDbContextFake = new Mock<IEddsDbContext>();

			_toggleProviderFake = new Mock<IToggleProvider>();

			_sut = new UserFieldSanitizer(serviceFactoryForAdmin.Object, _memoryCacheStub.Object,
				_eddsDbContextFake.Object, syncLog.Object, _toggleProviderFake.Object);
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

			_eddsDbContextFake.Verify(x => x.ExecuteSqlStatementAsScalar<int>(It.IsAny<string>()), Times.Never);

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
				x => x.RetrieveUsersBy(instanceArtifactId, It.Is<QueryRequest>(q => QueryWithUserArtifactId(q, instanceUserArtifactId))
					, It.IsAny<int>(), It.IsAny<int>()),
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
				x => x.RetrieveUsersBy(instanceArtifactId, It.Is<QueryRequest>(q => QueryWithUserArtifactId(q, workspaceUserArtifactId)),
					It.IsAny<int>(), It.IsAny<int>()),
				Times.Once);
			
			_userInfoManagerMock.Verify(
				x => x.RetrieveUsersBy(workspaceArtifactId, It.Is<QueryRequest>(q => QueryWithUserArtifactId(q, workspaceUserArtifactId)),
					It.IsAny<int>(), It.IsAny<int>()),
				Times.Once);
		}

		[Test]
		public async Task SanitizeAsync_ShouldThrowSyncItemLevelErrorException_WhenUserDoesNotExist()
		{
			// Arrange
			const int nonExistingUserArtifactId = 1;

			object initialValue = GetInitialValue(nonExistingUserArtifactId);

			// Act
			Func<Task> sanitizeAsync = () => _sut.SanitizeAsync(It.IsAny<int>(), 
				It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(), initialValue);

			// Assert
			await sanitizeAsync.Should().ThrowAsync<SyncItemLevelErrorException>().ConfigureAwait(false);
		}

		[Test]
		public async Task SanitizeAsync_ShouldReturnEmailAndAddToCache_WhenWorkspaceWasRestored()
		{
			// Arrange
			const int instanceArtifactId = -1;
			const int instanceUserArtifactId = 7;
			const string expectedUserEmail = "relativity.admin@kcura.com";

			const int restoredUserArtifactId = 9;
			object initialValue = GetInitialValue(restoredUserArtifactId);

			SetupRestoredUserToInstanceUserMap(restoredUserArtifactId, instanceUserArtifactId);

			SetupWorkspaceInfoManagerForUser(instanceArtifactId, instanceUserArtifactId, expectedUserEmail);

			// Act
			object sanitizedValue = await _sut.SanitizeAsync(
					It.IsAny<int>(), It.IsAny<string>(), It.IsAny<string>(),
					It.IsAny<string>(), initialValue)
				.ConfigureAwait(false);

			// Assert
			sanitizedValue.Should().Be(expectedUserEmail);

			_memoryCacheStub.Verify(x => x.Add(It.Is<string>(s => s.Contains(restoredUserArtifactId.ToString())),
				sanitizedValue, It.IsAny<CacheItemPolicy>()));
		}

		[Test]
		public async Task SanitizeAsync_ShouldGetEmailForInitialValue_WhenCheckingUserMapThrows()
		{
			// Arrange
			const int instanceArtifactId = -1;
			const int workspaceArtifactId = 100;

			const int restoredUserArtifactId = 9;
			object initialValue = GetInitialValue(restoredUserArtifactId);

			_eddsDbContextFake.Setup(x => x.ExecuteSqlStatementAsScalar<int>(It.IsAny<string>()))
				.Throws<Exception>();

			// Act
			Func<Task> sanitizeAsync = () => _sut.SanitizeAsync(
				workspaceArtifactId, It.IsAny<string>(), It.IsAny<string>(),
				It.IsAny<string>(), initialValue);

			// Assert
			await sanitizeAsync.Should().ThrowAsync<Exception>();

			_userInfoManagerMock.Verify(
				x => x.RetrieveUsersBy(instanceArtifactId, It.Is<QueryRequest>(q => QueryWithUserArtifactId(q, restoredUserArtifactId)),
					It.IsAny<int>(), It.IsAny<int>()),
				Times.Once);

			_userInfoManagerMock.Verify(
				x => x.RetrieveUsersBy(workspaceArtifactId, It.Is<QueryRequest>(q => QueryWithUserArtifactId(q, restoredUserArtifactId)),
					It.IsAny<int>(), It.IsAny<int>()),
				Times.Once);
		}

		[Test]
		public async Task SanitizeAsync_ShouldGetEmailForInitialValue_WhenInstanceUserWasNotFoundInMap()
		{
			// Arrange
			const int userNotFound = 0;

			const int instanceArtifactId = -1;
			const int workspaceArtifactId = 100;

			const int restoredUserArtifactId = 9;
			object initialValue = GetInitialValue(restoredUserArtifactId);

			SetupRestoredUserToInstanceUserMap(restoredUserArtifactId, userNotFound);

			// Act
			Func<Task> sanitizeAsync = () => _sut.SanitizeAsync(
				workspaceArtifactId, It.IsAny<string>(), It.IsAny<string>(),
				It.IsAny<string>(), initialValue);

			// Assert
			await sanitizeAsync.Should().ThrowAsync<Exception>();

			_userInfoManagerMock.Verify(
				x => x.RetrieveUsersBy(instanceArtifactId, It.Is<QueryRequest>(q => QueryWithUserArtifactId(q, restoredUserArtifactId)),
					It.IsAny<int>(), It.IsAny<int>()),
				Times.Once);

			_userInfoManagerMock.Verify(
				x => x.RetrieveUsersBy(workspaceArtifactId, It.Is<QueryRequest>(q => QueryWithUserArtifactId(q, restoredUserArtifactId)),
					It.IsAny<int>(), It.IsAny<int>()),
				Times.Once);
		}

		[Test]
		public async Task SanitizeAsync_ShouldGetEmailForInitialValueAndNotMapUser_WhenToggleIsEnabled()
		{
			// Arrange
			const int instanceArtifactId = -1;
			const int workspaceArtifactId = 100;

			const int restoredUserArtifactId = 9;
			object initialValue = GetInitialValue(restoredUserArtifactId);

			_toggleProviderFake.Setup(x => x.IsEnabledAsync<DisableUserMapWithSQL>())
				.ReturnsAsync(true);

			// Act
			Func<Task> sanitizeAsync = () => _sut.SanitizeAsync(
				workspaceArtifactId, It.IsAny<string>(), It.IsAny<string>(),
				It.IsAny<string>(), initialValue);

			// Assert
			await sanitizeAsync.Should().ThrowAsync<Exception>();

			_eddsDbContextFake.Verify(x => x.ExecuteSqlStatementAsScalar<int>(It.IsAny<string>()), Times.Never);

			_userInfoManagerMock.Verify(
				x => x.RetrieveUsersBy(instanceArtifactId, It.Is<QueryRequest>(q => QueryWithUserArtifactId(q, restoredUserArtifactId)),
					It.IsAny<int>(), It.IsAny<int>()),
				Times.Once);

			_userInfoManagerMock.Verify(
				x => x.RetrieveUsersBy(workspaceArtifactId, It.Is<QueryRequest>(q => QueryWithUserArtifactId(q, restoredUserArtifactId)),
					It.IsAny<int>(), It.IsAny<int>()),
				Times.Once);
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

		private void SetupRestoredUserToInstanceUserMap(int restoredUserId, int instanceUserId)
		{
			_eddsDbContextFake.Setup(x => x.ExecuteSqlStatementAsScalar<int>(
					It.Is<string>(s => s.Contains(restoredUserId.ToString()))))
				.Returns(instanceUserId);
		}

		private bool QueryWithUserArtifactId(QueryRequest reuqest, int userArtifactId)
		{
			return reuqest.Condition.Contains(userArtifactId.ToString());
		}
	}
}
