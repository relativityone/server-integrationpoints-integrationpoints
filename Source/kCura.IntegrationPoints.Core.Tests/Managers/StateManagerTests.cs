using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Managers.Implementations;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Domain.Models;
using NUnit.Framework;
using static kCura.IntegrationPoints.Core.Contracts.Configuration.SourceConfiguration;

namespace kCura.IntegrationPoints.Core.Tests.Managers
{
    [TestFixture, Category("Unit")]
    public class StateManagerTests : TestBase
    {
        [SetUp]
        public override void SetUp()
        {
            _instance = new StateManager();
        }

        private IStateManager _instance;

        [Test]
        public void GetRelativityProviderButtonState_ButtonsDisabled_JobsRunning()
        {
            // Arrange
            ExportType exportType = ExportType.SavedSearch;
            bool hasViewPermissions = false;
            bool hasProfileAddPermission = false;
            bool isCalculating = false;
            string lastJobHistoryStatus = "Processing";

            // Act
            ButtonStateDTO buttonStates = _instance.GetButtonState(
                exportType,
                ProviderType.Relativity,
                hasViewPermissions,
                hasProfileAddPermission,
                isCalculating,
                lastJobHistoryStatus);

            // Assert
            Assert.IsFalse(buttonStates.RunButtonEnabled);
            Assert.IsFalse(buttonStates.RetryErrorsButtonEnabled);
            Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);
            Assert.IsTrue(buttonStates.StopButtonEnabled);

            // Assert Visible
            Assert.IsTrue(buttonStates.RetryErrorsButtonVisible);
            Assert.IsFalse(buttonStates.ViewErrorsLinkVisible);
            Assert.IsFalse(buttonStates.SaveAsProfileButtonVisible);
        }

        [Test]
        public void GetRelativityProviderButtonState_GoldFlow_NoJobsRunning()
        {
            // Arrange
            ExportType exportType = ExportType.SavedSearch;
            bool hasViewPermissions = true;
            bool hasProfileAddPermission = false;
            bool isCalculating = false;
            string lastJobHistoryStatus = null;

            // Act
            ButtonStateDTO buttonStates = _instance.GetButtonState(
                exportType,
                ProviderType.Relativity,
                hasViewPermissions,
                hasProfileAddPermission,
                isCalculating,
                lastJobHistoryStatus);

            // Assert Enable
            Assert.IsTrue(buttonStates.RunButtonEnabled);
            Assert.IsFalse(buttonStates.RetryErrorsButtonEnabled);
            Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);
            Assert.IsFalse(buttonStates.StopButtonEnabled);
            Assert.IsFalse(buttonStates.DownloadErrorFileLinkEnabled);

            // Assert Visible
            Assert.IsTrue(buttonStates.RetryErrorsButtonVisible);
            Assert.IsTrue(buttonStates.ViewErrorsLinkVisible);
            Assert.IsFalse(buttonStates.SaveAsProfileButtonVisible);
            Assert.IsFalse(buttonStates.DownloadErrorFileLinkVisible);
        }

        [Test]
        public void GetRelativityProviderButtonState_HasErrors_JobsRunning()
        {
            // Arrange
            ExportType exportType = ExportType.SavedSearch;
            bool hasViewPermissions = false;
            bool hasProfileAddPermission = false;
            bool isCalculating = false;
            string lastJobHistoryStatus = "Validating";

            // Act
            ButtonStateDTO buttonStates = _instance.GetButtonState(
                exportType,
                ProviderType.Relativity,
                hasViewPermissions,
                hasProfileAddPermission,
                isCalculating,
                lastJobHistoryStatus);

            // Assert
            Assert.IsFalse(buttonStates.RunButtonEnabled);
            Assert.IsFalse(buttonStates.RetryErrorsButtonEnabled);
            Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);
            Assert.IsTrue(buttonStates.StopButtonEnabled);
            Assert.IsFalse(buttonStates.DownloadErrorFileLinkEnabled);

            // Assert Visible
            Assert.IsTrue(buttonStates.RetryErrorsButtonVisible);
            Assert.IsFalse(buttonStates.ViewErrorsLinkVisible);
            Assert.IsFalse(buttonStates.SaveAsProfileButtonVisible);
            Assert.IsFalse(buttonStates.DownloadErrorFileLinkVisible);
        }

        [Test]
        public void GetRelativityProviderButtonState_HasErrors_NoJobsRunning()
        {
            // Arrange
            ExportType exportType = ExportType.SavedSearch;
            bool hasViewPermissions = true;
            bool hasProfileAddPermission = false;
            bool isCalculating = false;
            string lastJobHistoryStatus = "Completed with errors";

            // Act
            ButtonStateDTO buttonStates = _instance.GetButtonState(
                exportType,
                ProviderType.Relativity,
                hasViewPermissions,
                hasProfileAddPermission,
                isCalculating,
                lastJobHistoryStatus);

            // Assert
            Assert.IsTrue(buttonStates.RunButtonEnabled);
            Assert.IsTrue(buttonStates.RetryErrorsButtonEnabled);
            Assert.IsTrue(buttonStates.ViewErrorsLinkEnabled);
            Assert.IsFalse(buttonStates.StopButtonEnabled);
            Assert.IsTrue(buttonStates.DownloadErrorFileLinkEnabled);

            // Assert Visible
            Assert.IsTrue(buttonStates.RetryErrorsButtonVisible);
            Assert.IsTrue(buttonStates.ViewErrorsLinkVisible);
            Assert.IsFalse(buttonStates.SaveAsProfileButtonVisible);
            Assert.IsFalse(buttonStates.DownloadErrorFileLinkVisible);
        }

        [Test]
        public void GetRelativityProviderButtonState_HasErrorsAndNoViewPermissions_NoJobsRunning()
        {
            // Arrange
            ExportType exportType = ExportType.SavedSearch;
            bool hasViewPermissions = false;
            bool hasProfileAddPermission = false;
            bool isCalculating = false;
            string lastJobHistoryStatus = "Completed with errors";

            // Act
            ButtonStateDTO buttonStates = _instance.GetButtonState(
                exportType,
                ProviderType.Relativity,
                hasViewPermissions,
                hasProfileAddPermission,
                isCalculating,
                lastJobHistoryStatus);

            // Assert
            Assert.IsTrue(buttonStates.RunButtonEnabled);
            Assert.IsTrue(buttonStates.RetryErrorsButtonEnabled);
            Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);
            Assert.IsFalse(buttonStates.StopButtonEnabled);
            Assert.IsTrue(buttonStates.DownloadErrorFileLinkEnabled);

            // Assert Visible
            Assert.IsTrue(buttonStates.RetryErrorsButtonVisible);
            Assert.IsFalse(buttonStates.ViewErrorsLinkVisible);
            Assert.IsFalse(buttonStates.SaveAsProfileButtonVisible);
            Assert.IsFalse(buttonStates.DownloadErrorFileLinkVisible);
        }

        [Test]
        public void GetRelativityProviderButtonState_HasProfileAddPermission_NoJobsRunning()
        {
            // Arrange
            ExportType exportType = ExportType.SavedSearch;
            bool hasViewPermissions = false;
            bool hasProfileAddPermission = true;
            bool isCalculating = false;
            string lastJobHistoryStatus = "Completed with errors";

            // Act
            ButtonStateDTO buttonStates = _instance.GetButtonState(
                exportType,
                ProviderType.Relativity,
                hasViewPermissions,
                hasProfileAddPermission,
                isCalculating,
                lastJobHistoryStatus);

            // Assert
            Assert.IsTrue(buttonStates.RunButtonEnabled);
            Assert.IsTrue(buttonStates.RetryErrorsButtonEnabled);
            Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);
            Assert.IsFalse(buttonStates.StopButtonEnabled);
            Assert.IsTrue(buttonStates.DownloadErrorFileLinkEnabled);

            // Assert Visible
            Assert.IsTrue(buttonStates.RetryErrorsButtonVisible);
            Assert.IsFalse(buttonStates.ViewErrorsLinkVisible);
            Assert.IsTrue(buttonStates.SaveAsProfileButtonVisible);
            Assert.IsFalse(buttonStates.DownloadErrorFileLinkVisible);
        }

        [Test]
        [TestCase(ProviderType.FTP)]
        [TestCase(ProviderType.LDAP)]
        [TestCase(ProviderType.LoadFile)]
        [TestCase(ProviderType.ImportLoadFile)]
        [TestCase(ProviderType.Other)]
        public void GetOtherProviderButtonState_HasProfileAddPermission_NoJobsRunning(ProviderType providerType)
        {
            // Arrange
            ExportType exportType = ExportType.SavedSearch;
            bool hasViewPermissions = false;
            bool hasProfileAddPermission = true;
            bool isCalculating = false;
            string lastJobHistoryStatus = "Completed with errors";

            // Act
            ButtonStateDTO buttonStates = _instance.GetButtonState(
                exportType,
                providerType,
                hasViewPermissions,
                hasProfileAddPermission,
                isCalculating,
                lastJobHistoryStatus);

            // Assert
            Assert.IsTrue(buttonStates.RunButtonEnabled);
            Assert.IsFalse(buttonStates.RetryErrorsButtonEnabled);
            Assert.IsFalse(buttonStates.ViewErrorsLinkEnabled);
            Assert.IsFalse(buttonStates.StopButtonEnabled);
            Assert.IsTrue(buttonStates.DownloadErrorFileLinkEnabled);

            // Assert Visible
            Assert.IsFalse(buttonStates.RetryErrorsButtonVisible);
            Assert.IsFalse(buttonStates.ViewErrorsLinkVisible);
            Assert.IsTrue(buttonStates.SaveAsProfileButtonVisible);
            Assert.IsTrue(buttonStates.DownloadErrorFileLinkVisible == (providerType == ProviderType.ImportLoadFile));
        }

        [TestCase(ProviderType.Relativity, ExportType.ProductionSet, true)]
        [TestCase(ProviderType.Relativity, ExportType.SavedSearch, true)]
        [TestCase(ProviderType.Relativity, ExportType.View, false)]
        public void GetRetryErrorsButtonState_WhenHasErrors(ProviderType providerType, ExportType exportType, bool expectedRetryErrorsVisibility)
        {
            // Arrange
            bool hasViewPermissions = true;
            bool hasProfileAddPermission = false;
            bool isCalculating = false;
            string lastJobHistoryStatus = "Pending";

            // Act
            ButtonStateDTO buttonStates = _instance.GetButtonState(
                exportType,
                providerType,
                hasViewPermissions,
                hasProfileAddPermission,
                isCalculating,
                lastJobHistoryStatus);

            // Assert
            buttonStates.RetryErrorsButtonVisible.Should().Be(expectedRetryErrorsVisibility);
        }

        [TestCase(true, false)]
        [TestCase(true, false)]
        public void GetCalculateStatisticsButtonState_AccordingToGivenCalculationState(bool isCalculationInProgress, bool calculateStatsButtonEnabled)
        {
            // Arrange
            ExportType exportType = ExportType.SavedSearch;
            ProviderType providerType = ProviderType.Relativity;
            bool hasViewPermissions = true;
            bool hasProfileAddPermission = false;
            bool isCalculating = isCalculationInProgress;
            string lastJobHistoryStatus = "Pending";

            // Act
            ButtonStateDTO buttonStates = _instance.GetButtonState(
                exportType,
                providerType,
                hasViewPermissions,
                hasProfileAddPermission,
                isCalculating,
                lastJobHistoryStatus);

            // Assert
            buttonStates.CalculateStatisticsButtonEnabled.Should().Be(calculateStatsButtonEnabled);
        }
    }
}
