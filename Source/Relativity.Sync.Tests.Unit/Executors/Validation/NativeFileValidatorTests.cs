using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Group;
using Relativity.Services.Objects;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Permission;
using Relativity.Services.User;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
	[TestFixture]
	public sealed class NativeFileValidatorTests
	{
		private NativeFileValidator _sut;

		private Mock<IInstanceSettings> _instanceSettings;
		private Mock<IUserContextConfiguration> _userContext;
		private Mock<ISourceServiceFactoryForAdmin> _serviceFactory;

		private Mock<IValidationConfiguration> _configuration;
		private Mock<IObjectManager> _objectManager;
		private Mock<IPermissionManager> _permissionManager;

		private const int _SOURCE_WORKSPACE_ID = 10000;
		private const int _USER_IS_ADMIN_ID = 1;
		private const int _USER_IS_NON_ADMIN_ID = 2;
		private const int _ADMIN_GROUP_ID = 100;

		private readonly QueryResult _SOURCE_WORKSPACE_ADMIN_GROUPS = new QueryResult()
		{
			Objects = new List<RelativityObject>()
			{
				new RelativityObject() { ArtifactID = _ADMIN_GROUP_ID },
			}
		};

		private readonly List<UserRef> _USERS_IN_ADMIN_GROUP = new List<UserRef>()
		{
			new UserRef(_USER_IS_ADMIN_ID)
		};

		[SetUp]
		public void SetUp()
		{
			_instanceSettings = new Mock<IInstanceSettings>();

			_userContext = new Mock<IUserContextConfiguration>();

			_objectManager = new Mock<IObjectManager>();
			_objectManager.Setup(m => m.QueryAsync(_SOURCE_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, Int32.MaxValue)).ReturnsAsync(_SOURCE_WORKSPACE_ADMIN_GROUPS);

			_permissionManager = new Mock<IPermissionManager>();
			_permissionManager.Setup(m => m.GetWorkspaceGroupUsersAsync(_SOURCE_WORKSPACE_ID, It.Is<GroupRef>(g => g.ArtifactID == _ADMIN_GROUP_ID)))
				.ReturnsAsync(_USERS_IN_ADMIN_GROUP);

			_serviceFactory = new Mock<ISourceServiceFactoryForAdmin>();
			_serviceFactory.Setup(s => s.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManager.Object);
			_serviceFactory.Setup(s => s.CreateProxyAsync<IPermissionManager>()).ReturnsAsync(_permissionManager.Object);

			_configuration = new Mock<IValidationConfiguration>();
			_configuration.Setup(c => c.SourceWorkspaceArtifactId).Returns(_SOURCE_WORKSPACE_ID);

			_sut = new NativeFileValidator(
				_instanceSettings.Object, 
				_userContext.Object, 
				_serviceFactory.Object, 
				new EmptyLogger());
		}

		[Test]
		public async Task ValidateAsync_ShouldHandleValidConfiguration_WhenConditionsAreMet()
		{
			//arrange
			_configuration.Setup(c => c.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.SetFileLinks);
			_instanceSettings.Setup(s => s.GetRestrictReferentialFileLinksOnImportAsync(default(bool))).ReturnsAsync(true);
			_userContext.Setup(c => c.ExecutingUserId).Returns(_USER_IS_ADMIN_ID);

			//act
			ValidationResult result = await _sut.ValidateAsync(_configuration.Object, CancellationToken.None).ConfigureAwait(false);

			//assert
			result.IsValid.Should().BeTrue();
			result.Messages.Should().BeEmpty();
		}

		[Test]
		public async Task ValidateAsync_ShouldHandleInvalidConfiguration_WhenUserIsNonAdmin()
		{
			//arrange
			_configuration.Setup(c => c.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.SetFileLinks);
			_instanceSettings.Setup(s => s.GetRestrictReferentialFileLinksOnImportAsync(default(bool))).ReturnsAsync(true);
			_userContext.Setup(c => c.ExecutingUserId).Returns(_USER_IS_NON_ADMIN_ID);

			//act
			ValidationResult result = await _sut.ValidateAsync(_configuration.Object, CancellationToken.None).ConfigureAwait(false);

			//assert
			result.IsValid.Should().BeFalse();
			result.Messages.Should().NotBeEmpty();
		}

		[Test]
		[TestCase(_USER_IS_ADMIN_ID)]
		[TestCase(_USER_IS_NON_ADMIN_ID)]
		public async Task ValidateAsync_ShouldSkipValidationIndependentOfUser_WhenResponsibleInstanceSettingIsFalse(int userId)
		{
			//arrange
			_configuration.Setup(c => c.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.SetFileLinks);
			_instanceSettings.Setup(s => s.GetRestrictReferentialFileLinksOnImportAsync(default(bool))).ReturnsAsync(false);
			_userContext.Setup(c => c.ExecutingUserId).Returns(userId);

			//act
			ValidationResult result = await _sut.ValidateAsync(_configuration.Object, CancellationToken.None).ConfigureAwait(false);

			//assert
			result.IsValid.Should().BeTrue();
			result.Messages.Should().BeEmpty();
		}

		[Test]
		[TestCase(ImportNativeFileCopyMode.DoNotImportNativeFiles)]
		[TestCase(ImportNativeFileCopyMode.CopyFiles)]
		public async Task ValidateAsync_ShouldSkipValidation_WhenNativeCopyModeIsNotFileLinks(ImportNativeFileCopyMode copyMode)
		{
			//arrange
			_configuration.Setup(c => c.ImportNativeFileCopyMode).Returns(copyMode);
			_instanceSettings.Setup(s => s.GetRestrictReferentialFileLinksOnImportAsync(default(bool))).ReturnsAsync(false);
			_userContext.Setup(c => c.ExecutingUserId).Returns(_USER_IS_NON_ADMIN_ID);

			//act
			ValidationResult result = await _sut.ValidateAsync(_configuration.Object, CancellationToken.None).ConfigureAwait(false);

			//assert
			result.IsValid.Should().BeTrue();
			result.Messages.Should().BeEmpty();
		}
	}
}
