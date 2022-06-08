using System;
using System.Threading.Tasks;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Sync.Authentication;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Toggles;
using Relativity.Sync.Toggles.Service;
using IExtendedImportAPI = Relativity.Sync.Executors.IExtendedImportAPI;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class ImportAPIFactoryTests
	{
		private ImportApiFactory _sut;
		private Mock<IUserContextConfiguration> _userContextConfigurationFake;
		private Mock<IAuthTokenGenerator> _tokenGeneratorFake;
		private Mock<ISyncToggles> _syncTogglesFake;
		private Mock<IUserService> _userServiceFake;
		private Mock<IAPILog> _syncLogMock;
		private Mock<IExtendedImportAPI> _extendedImportAPIFake;
		private const int _GLOBAL_ADMIN_USER_ID = 777;
		private const int _USER_IS_ADMIN_ID = 666;
		private const int _USER_IS_NON_ADMIN_ID = 111;
		private static readonly Uri _WEB_SERVICE_URI = new Uri("http://www.rip.com/");


		[SetUp]
		public void SetUp()
		{
			_userContextConfigurationFake = new Mock<IUserContextConfiguration>();
			_userServiceFake = new Mock<IUserService>();
			_tokenGeneratorFake = new Mock<IAuthTokenGenerator>();
			_syncTogglesFake = new Mock<ISyncToggles>();
			_extendedImportAPIFake = new Mock<IExtendedImportAPI>();
			_syncLogMock = new Mock<IAPILog>();

			_sut = new ImportApiFactory(
				_userContextConfigurationFake.Object,
				_tokenGeneratorFake.Object,
				_syncTogglesFake.Object,
				_userServiceFake.Object,
				_extendedImportAPIFake.Object,
				_syncLogMock.Object
				);
		}
		
		[TestCase(_USER_IS_ADMIN_ID)]
		[TestCase(_USER_IS_NON_ADMIN_ID)]
		public async Task CreateImportApiAsync_ShouldCreateIapiWithUserID_WhenToggleIsDisabled(int userId)
		{
			//ARRANGE
			_userContextConfigurationFake.SetupGet(x => x.ExecutingUserId).Returns(userId);
			
			//ACT
			await _sut.CreateImportApiAsync(_WEB_SERVICE_URI).ConfigureAwait(false);

			// ASSERT
			_syncLogMock.Verify(x => x.LogInformation("Creating IAPI as userId: {executingUserId}", userId));
		}
		
		[Test]
		public async Task CreateImportApiAsync_ShouldCreateIapiWithGlobalAdminUserID_WhenToggleIsEnabledAndUserIsNotAdmin()
		{
			//ARRANGE
			int userId = _USER_IS_NON_ADMIN_ID;
			_userContextConfigurationFake.SetupGet(x => x.ExecutingUserId).Returns(userId);
			_userServiceFake.Setup(x => x.ExecutingUserIsAdminAsync(It.IsAny<int>()))
				.Returns(Task.FromResult(false));
			_syncTogglesFake.Setup(x => x.IsEnabled<EnableNonAdminSyncLinksToggle>()).Returns(true);
			
			//ACT
			await _sut.CreateImportApiAsync(_WEB_SERVICE_URI).ConfigureAwait(false);

			// ASSERT
			_syncLogMock.Verify(x => x.LogInformation("Creating IAPI as userId: {executingUserId}", _GLOBAL_ADMIN_USER_ID));
		}
		
		[Test]
		public async Task CreateImportApiAsync_ShouldCreateIapiWithAdminUserID_WhenToggleIsEnabledAndUserAdmin()
		{
			//ARRANGE
			int userId = _USER_IS_ADMIN_ID;
			_userContextConfigurationFake.SetupGet(x => x.ExecutingUserId).Returns(userId);
			_userServiceFake.Setup(x => x.ExecutingUserIsAdminAsync(It.IsAny<int>()))
				.Returns(Task.FromResult(true));
			_syncTogglesFake.Setup(x => x.IsEnabled<EnableNonAdminSyncLinksToggle>()).Returns(true);
			
			//ACT
			await _sut.CreateImportApiAsync(_WEB_SERVICE_URI).ConfigureAwait(false);

			// ASSERT
			_syncLogMock.Verify(x => x.LogInformation("Creating IAPI as userId: {executingUserId}", _USER_IS_ADMIN_ID));
		}

	}
}