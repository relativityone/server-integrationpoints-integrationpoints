using System;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.Logging;
using Relativity.Sync.Pipelines;
using Relativity.Sync.Tests.Common.Attributes;
using Relativity.Sync.Toggles;
using Relativity.Sync.Toggles.Service;

namespace Relativity.Sync.Tests.Unit.Executors.Validation
{
    [TestFixture]
    public sealed class ImageCopyLinksValidatorTests
    {
        private ImageCopyLinksValidator _sut;

        private Mock<IInstanceSettings> _instanceSettingsFake;
        private Mock<IUserContextConfiguration> _userContextFake;

        private Mock<IValidationConfiguration> _configurationFake;
        private Mock<ISyncToggles> _nonAdminCanSyncUsingLinksFake;
        private Mock<IUserService> _userServiceFake;

        private const int _USER_IS_ADMIN_ID = 1;
        private const int _USER_IS_NON_ADMIN_ID = 2;

        [SetUp]
        public void SetUp()
        {
            _instanceSettingsFake = new Mock<IInstanceSettings>();
            _userContextFake = new Mock<IUserContextConfiguration>();

            _configurationFake = new Mock<IValidationConfiguration>();

            _nonAdminCanSyncUsingLinksFake = new Mock<ISyncToggles>();
            _userServiceFake = new Mock<IUserService>();

            _sut = new ImageCopyLinksValidator(
                _instanceSettingsFake.Object,
                _userContextFake.Object,
                _nonAdminCanSyncUsingLinksFake.Object,
                _userServiceFake.Object,
                new EmptyLogger());
        }

        [Test]
        public async Task ValidateAsync_ShouldHandleValidConfiguration_WhenConditionsAreMet()
        {
            // Arrange
            SetupValidator(_USER_IS_ADMIN_ID, ImportImageFileCopyMode.SetFileLinks, true, false);

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
            SetupValidator(_USER_IS_NON_ADMIN_ID, ImportImageFileCopyMode.SetFileLinks, true, false);

            // Act
            ValidationResult result = await _sut.ValidateAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.IsValid.Should().BeFalse();
            result.Messages.Should().NotBeEmpty();
        }

        [TestCase(_USER_IS_ADMIN_ID)]
        [TestCase(_USER_IS_NON_ADMIN_ID)]
        public async Task ValidateAsync_ShouldSkipValidationIndependentOfUser_WhenResponsibleInstanceSettingIsFalse(int userId)
        {
            // Arrange
            SetupValidator(userId, ImportImageFileCopyMode.SetFileLinks, false, false);

            // Act
            ValidationResult result = await _sut.ValidateAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Messages.Should().BeEmpty();
        }

        [TestCase(ImportImageFileCopyMode.CopyFiles)]
        public async Task ValidateAsync_ShouldSkipValidation_WhenNativeCopyModeIsNotFileLinks(ImportImageFileCopyMode copyMode)
        {
            // Arrange
            SetupValidator(_USER_IS_NON_ADMIN_ID, copyMode, true, false);

            // Act
            ValidationResult result = await _sut.ValidateAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Messages.Should().BeEmpty();
        }

        [TestCase(typeof(SyncDocumentRunPipeline), false)]
        [TestCase(typeof(SyncDocumentRetryPipeline), false)]
        [TestCase(typeof(SyncImageRunPipeline), true)]
        [TestCase(typeof(SyncImageRetryPipeline), true)]
        [TestCase(typeof(SyncNonDocumentRunPipeline), false)]
        [EnsureAllPipelineTestCase(0)]
        public void ShouldExecute_ShouldReturnCorrectValue(Type pipelineType, bool expectedResult)
        {
            // Arrange
            ISyncPipeline pipelineObject = (ISyncPipeline)Activator.CreateInstance(pipelineType);

            // Act
            bool actualResult = _sut.ShouldValidate(pipelineObject);

            // Assert
            actualResult.Should().Be(
                expectedResult,
                $"ShouldValidate should return {expectedResult} for pipeline {pipelineType.Name}");
        }

        [Test]
        public async Task ValidateAsync_ShouldSkipValidationOfUser_WhenNonAdminCanSyncUsingLinkToggleIsTrue()
        {
            // Arrange
            SetupValidator(_USER_IS_NON_ADMIN_ID, ImportImageFileCopyMode.SetFileLinks, true, true);

            // Act
            ValidationResult result = await _sut.ValidateAsync(_configurationFake.Object, CancellationToken.None).ConfigureAwait(false);

            // Assert
            result.IsValid.Should().BeTrue();
            result.Messages.Should().BeEmpty();
        }

        private void SetupValidator(int userId, ImportImageFileCopyMode copyMode, bool isRestrictedCopyLinksOnly, bool toggleNonAdminCanSyncUsingLinks)
        {
            _userContextFake.Setup(c => c.ExecutingUserId).Returns(userId);
            _configurationFake.Setup(c => c.ImportImageFileCopyMode).Returns(copyMode);
            _instanceSettingsFake.Setup(s => s.GetRestrictReferentialFileLinksOnImportAsync(default(bool))).ReturnsAsync(isRestrictedCopyLinksOnly);
            _nonAdminCanSyncUsingLinksFake.Setup(t => t.IsEnabled<EnableNonAdminSyncLinksToggle>()).Returns(toggleNonAdminCanSyncUsingLinks);
            _userServiceFake.Setup(u => u.ExecutingUserIsAdminAsync(It.IsAny<int>())).Returns(Task.FromResult(userId == _USER_IS_ADMIN_ID));
        }
    }
}
