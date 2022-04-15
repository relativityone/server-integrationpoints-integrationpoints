using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Interfaces.Group;
using Relativity.Services.Objects.DataContracts;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Tests.Common.Attributes;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
	[TestFixture]
	public sealed class NativeCopyLinksValidatorTests
	{
		private NativeCopyLinksValidator _sut;

		private Mock<IInstanceSettings> _instanceSettingsFake;
		private Mock<IUserContextConfiguration> _userContextFake;
		private Mock<ISourceServiceFactoryForAdmin> _serviceFactoryForAdminFake;

		private Mock<IValidationConfiguration> _configurationFake;
		private Mock<IGroupManager> _groupManagerFake;

		private const int _USER_IS_ADMIN_ID = 1;
		private const int _USER_IS_NON_ADMIN_ID = 2;

		[SetUp]
		public void SetUp()
		{
			_instanceSettingsFake = new Mock<IInstanceSettings>();
			_userContextFake = new Mock<IUserContextConfiguration>();
			_groupManagerFake = new Mock<IGroupManager>();

			_serviceFactoryForAdminFake = new Mock<ISourceServiceFactoryForAdmin>();
			_serviceFactoryForAdminFake.Setup(s => s.CreateProxyAsync<IGroupManager>()).ReturnsAsync(_groupManagerFake.Object);

			_configurationFake = new Mock<IValidationConfiguration>();

			_sut = new NativeCopyLinksValidator(
				_instanceSettingsFake.Object,
				_userContextFake.Object,
				_serviceFactoryForAdminFake.Object,
				new EmptyLogger());
		}

		[Test]
		public async Task ValidateAsync_ShouldHandleValidConfiguration_WhenConditionsAreMet()
		{
			// Arrange
			SetupValidator(_USER_IS_ADMIN_ID, ImportNativeFileCopyMode.SetFileLinks, true);

			// Act
			ValidationResult result = await _sut.ValidateAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.IsValid.Should().BeTrue();
			result.Messages.Should().BeEmpty();
		}

		[Test]
		public async Task ValidateAsync_ShouldHandleInvalidConfiguration_WhenUserIsNonAdmin()
		{
			// Arrange
			SetupValidator(_USER_IS_NON_ADMIN_ID, ImportNativeFileCopyMode.SetFileLinks, true);

			// Act
			ValidationResult result = await _sut.ValidateAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.IsValid.Should().BeFalse();
			result.Messages.Should().NotBeEmpty();
		}

		[Test]
		[TestCase(_USER_IS_ADMIN_ID)]
		[TestCase(_USER_IS_NON_ADMIN_ID)]
		public async Task ValidateAsync_ShouldSkipValidationIndependentOfUser_WhenResponsibleInstanceSettingIsFalse(int userId)
		{
			// Arrange
			SetupValidator(userId, ImportNativeFileCopyMode.SetFileLinks, false);

			// Act
			ValidationResult result = await _sut.ValidateAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.IsValid.Should().BeTrue();
			result.Messages.Should().BeEmpty();
		}

		[Test]
		[TestCase(ImportNativeFileCopyMode.DoNotImportNativeFiles)]
		[TestCase(ImportNativeFileCopyMode.CopyFiles)]
		public async Task ValidateAsync_ShouldSkipValidation_WhenNativeCopyModeIsNotFileLinks(ImportNativeFileCopyMode copyMode)
		{
			// Arrange
			SetupValidator(_USER_IS_NON_ADMIN_ID, copyMode, true);

			// Act
			ValidationResult result = await _sut.ValidateAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

			// Assert
			result.IsValid.Should().BeTrue();
			result.Messages.Should().BeEmpty();
		}

		[TestCase(typeof(SyncDocumentRunPipeline), true)]
		[TestCase(typeof(SyncDocumentRetryPipeline), true)]
		[TestCase(typeof(SyncImageRunPipeline), false)]
		[TestCase(typeof(SyncImageRetryPipeline), false)]
		[TestCase(typeof(SyncNonDocumentRunPipeline), false)]
		[EnsureAllPipelineTestCase(0)]
		public void ShouldExecute_ShouldReturnCorrectValue(Type pipelineType, bool expectedResult)
		{
			// Arrange
			ISyncPipeline pipelineObject = (ISyncPipeline)Activator.CreateInstance(pipelineType);

			// Act
			bool actualResult = _sut.ShouldValidate(pipelineObject);

			// Assert
			actualResult.Should().Be(expectedResult,
				$"ShouldValidate should return {expectedResult} for pipeline {pipelineType.Name}");
		}

		private void SetupValidator(int userId, ImportNativeFileCopyMode copyMode, bool isRestrictedCopyLinksOnly)
		{
			_userContextFake.Setup(c => c.ExecutingUserId).Returns(userId);
			_configurationFake.Setup(c => c.ImportNativeFileCopyMode).Returns(copyMode);
			_instanceSettingsFake.Setup(s => s.GetRestrictReferentialFileLinksOnImportAsync(default(bool))).ReturnsAsync(isRestrictedCopyLinksOnly);

			List<RelativityObjectSlim> groups = userId == _USER_IS_ADMIN_ID
				? new List<RelativityObjectSlim> { new RelativityObjectSlim() }
				: new List<RelativityObjectSlim>();
			QueryResultSlim queryResultSlim = new QueryResultSlim { Objects = groups };

			_groupManagerFake.Setup(m => m.QueryGroupsByUserAsync(It.IsAny<QueryRequest>(), It.IsAny<int>(), It.IsAny<int>(), userId))
				.Returns(Task.FromResult(queryResultSlim));
		}
	}
}