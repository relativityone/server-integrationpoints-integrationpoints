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
	public sealed class NativeCopyLinksValidatorTests
	{
		private NativeCopyLinksValidator _sut;

		private Mock<IInstanceSettings> _instanceSettingsFake;
		private Mock<IUserContextConfiguration> _userContextFake;
		private Mock<ISourceServiceFactoryForAdmin> _serviceFactoryFake;

		private Mock<IValidationConfiguration> _configurationFake;
		private Mock<IObjectManager> _objectManagerFake;
		private Mock<IPermissionManager> _permissionManagerFake;

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
			_instanceSettingsFake = new Mock<IInstanceSettings>();

			_userContextFake = new Mock<IUserContextConfiguration>();

			_objectManagerFake = new Mock<IObjectManager>();
			_objectManagerFake.Setup(m => m.QueryAsync(_SOURCE_WORKSPACE_ID, It.IsAny<QueryRequest>(), 0, It.IsAny<int>())).ReturnsAsync(_SOURCE_WORKSPACE_ADMIN_GROUPS);

			_permissionManagerFake = new Mock<IPermissionManager>();
			_permissionManagerFake.Setup(m => m.GetWorkspaceGroupUsersAsync(_SOURCE_WORKSPACE_ID, It.Is<GroupRef>(g => g.ArtifactID == _ADMIN_GROUP_ID)))
				.ReturnsAsync(_USERS_IN_ADMIN_GROUP);

			_serviceFactoryFake = new Mock<ISourceServiceFactoryForAdmin>();
			_serviceFactoryFake.Setup(s => s.CreateProxyAsync<IObjectManager>()).ReturnsAsync(_objectManagerFake.Object);
			_serviceFactoryFake.Setup(s => s.CreateProxyAsync<IPermissionManager>()).ReturnsAsync(_permissionManagerFake.Object);

			_configurationFake = new Mock<IValidationConfiguration>();
			_configurationFake.Setup(c => c.SourceWorkspaceArtifactId).Returns(_SOURCE_WORKSPACE_ID);

			_sut = new NativeCopyLinksValidator(
				_instanceSettingsFake.Object, 
				_userContextFake.Object, 
				_serviceFactoryFake.Object, 
				new EmptyLogger());
		}

		[Test]
		public async Task ValidateAsync_ShouldHandleValidConfiguration_WhenConditionsAreMet()
		{
			//arrange
			_configurationFake.Setup(c => c.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.SetFileLinks);
			_instanceSettingsFake.Setup(s => s.GetRestrictReferentialFileLinksOnImportAsync(default(bool))).ReturnsAsync(true);
			_userContextFake.Setup(c => c.ExecutingUserId).Returns(_USER_IS_ADMIN_ID);

			//act
			ValidationResult result = await _sut.ValidateAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

			//assert
			result.IsValid.Should().BeTrue();
			result.Messages.Should().BeEmpty();
		}

		[Test]
		public async Task ValidateAsync_ShouldHandleInvalidConfiguration_WhenUserIsNonAdmin()
		{
			//arrange
			_configurationFake.Setup(c => c.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.SetFileLinks);
			_instanceSettingsFake.Setup(s => s.GetRestrictReferentialFileLinksOnImportAsync(default(bool))).ReturnsAsync(true);
			_userContextFake.Setup(c => c.ExecutingUserId).Returns(_USER_IS_NON_ADMIN_ID);

			//act
			ValidationResult result = await _sut.ValidateAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

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
			_configurationFake.Setup(c => c.ImportNativeFileCopyMode).Returns(ImportNativeFileCopyMode.SetFileLinks);
			_instanceSettingsFake.Setup(s => s.GetRestrictReferentialFileLinksOnImportAsync(default(bool))).ReturnsAsync(false);
			_userContextFake.Setup(c => c.ExecutingUserId).Returns(userId);

			//act
			ValidationResult result = await _sut.ValidateAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

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
			_configurationFake.Setup(c => c.ImportNativeFileCopyMode).Returns(copyMode);
			_instanceSettingsFake.Setup(s => s.GetRestrictReferentialFileLinksOnImportAsync(default(bool))).ReturnsAsync(false);
			_userContextFake.Setup(c => c.ExecutingUserId).Returns(_USER_IS_NON_ADMIN_ID);

			//act
			ValidationResult result = await _sut.ValidateAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

			//assert
			result.IsValid.Should().BeTrue();
			result.Messages.Should().BeEmpty();
		}
	}
}
