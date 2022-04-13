using System;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Authentication;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Transfer;

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
		private const int _USER_IS_ADMIN_ID = 777;
		private const int _USER_IS_NON_ADMIN_ID = 111;
		
		
		[SetUp]
		public void SetUp()
		{
			_userContextConfigurationFake = new Mock<IUserContextConfiguration>();
			_userServiceFake = new Mock<IUserService>();
			_tokenGeneratorFake = new Mock<IAuthTokenGenerator>();
			_nonAdminCanSyncUsingLinksFake = new Mock<INonAdminCanSyncUsingLinks>();
			_syncLogMock = new Mock<ISyncLog>();
			
			_sut = new ImportApiFactory(
				_userContextConfigurationFake.Object,
				_tokenGeneratorFake.Object,
				_nonAdminCanSyncUsingLinksFake.Object,
				_userServiceFake.Object,
				_syncLogMock.Object
				);
		}

		[Test]
		[TestCase(_USER_IS_ADMIN_ID)]
		[TestCase(_USER_IS_NON_ADMIN_ID)]
		public void ShouldDo_WhenSomething(int userId)
		{
			//ARRANGE
			
			//ACT
			var aa = _sut.CreateImportApiAsync(It.IsAny<Uri>()).ConfigureAwait(false);
			//ASSERT
			_syncLogMock.Verify(x => x.LogInformation(It.Is<string>(s=>s.Contains(userId.ToString()))));
			
		}
	}
}