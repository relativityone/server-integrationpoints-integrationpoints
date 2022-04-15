using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.Relativity.ImportAPI;
using kCura.WinEDDS.Exceptions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Authentication;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Transfer;
using IExtendedImportAPI = Relativity.Sync.Executors.IExtendedImportAPI;

namespace Relativity.Sync.Tests.Unit
{
	[TestFixture]
	public sealed class ImportAPIFactoryTests
	{
		private ImportApiFactory _sut;
		private Mock<IUserContextConfiguration> _userContextConfigurationFake;
		private Mock<IAuthTokenGenerator> _tokenGeneratorFake;
		private Mock<INonAdminCanSyncUsingLinks> _nonAdminCanSyncUsingLinksFake;
		private Mock<IUserService> _userServiceFake;
		private Mock<ISyncLog> _syncLogMock;
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
			_nonAdminCanSyncUsingLinksFake = new Mock<INonAdminCanSyncUsingLinks>();
			_extendedImportAPIFake = new Mock<IExtendedImportAPI>();
			_syncLogMock = new Mock<ISyncLog>();

			_sut = new ImportApiFactory(
				_userContextConfigurationFake.Object,
				_tokenGeneratorFake.Object,
				_nonAdminCanSyncUsingLinksFake.Object,
				_userServiceFake.Object,
				_extendedImportAPIFake.Object,
				_syncLogMock.Object
				);
		}
		
		[TestCase(_USER_IS_ADMIN_ID)]
		[TestCase(_USER_IS_NON_ADMIN_ID)]
		public void CreateImportApiAsync_ShouldCreateIapiWithUserID_WhenToggleIsDisabled(int userId)
		{
			//ARRANGE
			_userContextConfigurationFake.SetupGet(x => x.ExecutingUserId).Returns(userId);
			
			//ACT
			_sut.CreateImportApiAsync(_WEB_SERVICE_URI);

			// ASSERT
			_syncLogMock.Verify(x => x.LogInformation("Creating IAPI as userId: {executingUserId}", userId));
		}
		
		[Test]
		public void CreateImportApiAsync_ShouldCreateIapiWithGlobalAdminUserID_WhenToggleIsEnabledAndUserIsNotAdmin()
		{
			//ARRANGE
			int userId = _USER_IS_NON_ADMIN_ID;
			_userContextConfigurationFake.SetupGet(x => x.ExecutingUserId).Returns(userId);
			_userServiceFake.Setup(x => x.ExecutingUserIsAdminAsync(It.IsAny<IUserContextConfiguration>()))
				.Returns(Task.FromResult(false));
			_nonAdminCanSyncUsingLinksFake.Setup(x => x.IsEnabled()).Returns(true);
			
			//ACT
			_sut.CreateImportApiAsync(_WEB_SERVICE_URI);

			// ASSERT
			_syncLogMock.Verify(x => x.LogInformation("Creating IAPI as userId: {executingUserId}", _GLOBAL_ADMIN_USER_ID));
		}
		
		[Test]
		public void CreateImportApiAsync_ShouldCreateIapiWithAdminUserID_WhenToggleIsEnabledAndUserAdmin()
		{
			//ARRANGE
			int userId = _USER_IS_ADMIN_ID;
			_userContextConfigurationFake.SetupGet(x => x.ExecutingUserId).Returns(userId);
			_userServiceFake.Setup(x => x.ExecutingUserIsAdminAsync(It.IsAny<IUserContextConfiguration>()))
				.Returns(Task.FromResult(true));
			_nonAdminCanSyncUsingLinksFake.Setup(x => x.IsEnabled()).Returns(true);
			
			//ACT
			_sut.CreateImportApiAsync(_WEB_SERVICE_URI);

			// ASSERT
			_syncLogMock.Verify(x => x.LogInformation("Creating IAPI as userId: {executingUserId}", _USER_IS_ADMIN_ID));
		}

	}
}