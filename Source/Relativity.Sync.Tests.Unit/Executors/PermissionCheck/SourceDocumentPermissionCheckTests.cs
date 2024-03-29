using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.API;
using Relativity.Services.Permission;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.PermissionCheck;
using Relativity.Sync.Executors.PermissionCheck.DocumentPermissionChecks;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.RDOs;

namespace Relativity.Sync.Tests.Unit.Executors.PermissionCheck
{
    [TestFixture]
    public class SourceDocumentPermissionCheckTests
    {
        private SourceDocumentPermissionCheck _instance;

        private Mock<IAPILog> _logger;
        private Mock<ISourceServiceFactoryForUser> _serviceFactoryForUser;

        private const int _TEST_WORKSPACE_ARTIFACT_ID = 105789;
        private const int _ALLOW_EXPORT_PERMISSION_ID = 159; // 159 is the artifact id of the "Allow Export" permission
        private const int _EDIT_DOCUMENT_PERMISSION_ID = 45; // 45 is the artifact id of the "Edit Documents" permission
        private const int _EXPECTED_VALUE_FOR_ALL_FAILED_VALIDATE = 8;
        private const int _ARTIFACT_OBJECT_TYPE = 25;

        private readonly Guid _jobHistory = new Guid("08f4b1f7-9692-4a08-94ab-b5f3a88b6cc9");
        private readonly Guid _batchObjectTypeGuid = new Guid("18C766EB-EB71-49E4-983E-FFDE29B1A44E");
        private readonly Guid _progressObjectTypeGuid = new Guid("3D107450-DB18-4FE1-8219-73EE1F921ED9");

        [SetUp]
        public void SetUp()
        {
            _logger = new Mock<IAPILog>();
            _serviceFactoryForUser = new Mock<ISourceServiceFactoryForUser>();
            _instance = new SourceDocumentPermissionCheck(_logger.Object, _serviceFactoryForUser.Object);
        }

        [Test]
        public async Task ValidateAsyncGoldFlowTest()
        {
            // Arrange
            Mock<IPermissionsCheckConfiguration> configuration = ConfigurationSet();
            ArrangeSet();

            // Act
            ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object).ConfigureAwait(false);

            // Assert
            actualResult.IsValid.Should().BeTrue();
            actualResult.Messages.Should().HaveCount(0);
        }

        [Test]
        public async Task UserShouldNotHavePermissionToAccessThisWorkspaceTest()
        {
            // Arrange
            Mock<IPermissionsCheckConfiguration> configuration = ConfigurationSet();
            Mock<IPermissionManager> permissionManager = ArrangeSet();

            permissionManager.Setup(x => x.GetPermissionSelectedAsync(
                -1,
                It.IsAny<List<PermissionRef>>(), It.IsAny<int>())).Throws<SyncException>();

            // Act
            ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object).ConfigureAwait(false);

            // Assert
            actualResult.IsValid.Should().BeFalse();
            actualResult.Messages.Should().HaveCount(1);
            actualResult.Messages.First().ShortMessage.Should()
                .Be("User does not have permission to access the source workspace.");
        }

        [Test]
        public async Task UserShouldNotHavePermissionToAddJobHistoryRdoTest()
        {
            // Arrange
            Mock<IPermissionsCheckConfiguration> configuration = ConfigurationSet();
            Mock<IPermissionManager> permissionManager = ArrangeSet();

            permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.ArtifactType.Guids.Contains(_jobHistory)))))
                .Throws<SyncException>();

            // Act
            ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object).ConfigureAwait(false);

            // Assert
            actualResult.IsValid.Should().BeFalse();
            actualResult.Messages.Should().HaveCount(1);
            actualResult.Messages.First().ShortMessage.Should()
                .Be(LackOfPermissionMessages._JOB_HISTORY_TYPE_NO_ADD);
        }

        [Test]
        public async Task UserShouldNotHavePermissionToAddObjectTypeTest()
        {
            // Arrange
            Mock<IPermissionsCheckConfiguration> configuration = ConfigurationSet();
            Mock<IPermissionManager> permissionManager = ArrangeSet();

            permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.ArtifactType.ID == _ARTIFACT_OBJECT_TYPE))))
                .Throws<SyncException>();

            // Act
            ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object).ConfigureAwait(false);

            // Assert
            actualResult.IsValid.Should().BeFalse();
            actualResult.Messages.Should().HaveCount(1);
            actualResult.Messages.First().ShortMessage.Should()
                .Be(LackOfPermissionMessages._OBJECT_TYPE_NO_ADD);
        }

        [Test]
        public async Task UserShouldNotHavePermissionToBatchTest()
        {
            // Arrange
            Mock<IPermissionsCheckConfiguration> configuration = ConfigurationSet();
            Mock<IPermissionManager> permissionManager = ArrangeSet();

            permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.ArtifactType.Guids.Contains(_batchObjectTypeGuid)))))
                .Throws<SyncException>();

            // Act
            ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object).ConfigureAwait(false);

            // Assert
            actualResult.IsValid.Should().BeFalse();
            actualResult.Messages.Should().HaveCount(1);
            actualResult.Messages.First().ShortMessage.Should()
                .Be(LackOfPermissionMessages._BATCH_OBJECT_TYPE_ERROR);
        }

        [Test]
        public async Task UserShouldNotHavePermissionToProgressTest()
        {
            // Arrange
            Mock<IPermissionsCheckConfiguration> configuration = ConfigurationSet();
            Mock<IPermissionManager> permissionManager = ArrangeSet();

            permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.ArtifactType.Guids.Contains(_progressObjectTypeGuid)))))
                .Throws<SyncException>();

            // Act
            ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object).ConfigureAwait(false);

            // Assert
            actualResult.IsValid.Should().BeFalse();
            actualResult.Messages.Should().HaveCount(1);
            actualResult.Messages.First().ShortMessage.Should()
                .Be(LackOfPermissionMessages._PROGRESS_OBJECT_TYPE_ERROR);
        }

        [Test]
        public async Task UserShouldNotHavePermissionToConfigurationTest()
        {
            // Arrange
            Mock<IPermissionsCheckConfiguration> configuration = ConfigurationSet();
            Mock<IPermissionManager> permissionManager = ArrangeSet();

            permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.ArtifactType.Guids.Contains(new Guid(SyncRdoGuids.SyncConfigurationGuid))))))
                .Throws<SyncException>();

            // Act
            ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object).ConfigureAwait(false);

            // Assert
            actualResult.IsValid.Should().BeFalse();
            actualResult.Messages.Should().HaveCount(1);
            actualResult.Messages.First().ShortMessage.Should()
                .Be(LackOfPermissionMessages._CONFIGURATION_TYPE_NO_ADD);
        }

        [Test]
        public async Task UserShouldNotHavePermissionToExportInTheSourceWorkspaceTest()
        {
            // Arrange
            Mock<IPermissionsCheckConfiguration> configuration = ConfigurationSet();
            Mock<IPermissionManager> permissionManager = ArrangeSet();

            permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _ALLOW_EXPORT_PERMISSION_ID))))
                .Throws<SyncException>();

            // Act
            ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object).ConfigureAwait(false);

            // Assert
            actualResult.IsValid.Should().BeFalse();
            actualResult.Messages.Should().HaveCount(1);
            actualResult.Messages.First().ShortMessage.Should()
                .Be(LackOfPermissionMessages._SOURCE_WORKSPACE_NO_EXPORT);
        }

        [Test]
        public async Task UserShouldNotHavePermissionToEditDocumentsInThisWorkspaceTest()
        {
            // Arrange
            Mock<IPermissionsCheckConfiguration> configuration = ConfigurationSet();
            Mock<IPermissionManager> permissionManager = ArrangeSet();

            permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _EDIT_DOCUMENT_PERMISSION_ID))))
                .Throws<SyncException>();

            // Act
            ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object).ConfigureAwait(false);

            // Assert
            actualResult.IsValid.Should().BeFalse();
            actualResult.Messages.Should().HaveCount(1);
            actualResult.Messages.First().ShortMessage.Should()
                .Be("User does not have permission to edit Documents in this workspace.");
        }

        [Test]
        public async Task ExecuteAllowExportPermissionTest()
        {
            // Arrange
            Mock<IPermissionsCheckConfiguration> configuration = ConfigurationSet();
            Mock<IPermissionManager> permissionManager = ArrangeSet();

            var permissionToExport = new List<PermissionValue> { new PermissionValue { PermissionID = _ALLOW_EXPORT_PERMISSION_ID, Selected = false } };
            permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _ALLOW_EXPORT_PERMISSION_ID))))
                .ReturnsAsync(permissionToExport);

            // Act
            ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object).ConfigureAwait(false);

            // Assert
            actualResult.IsValid.Should().BeFalse();
            actualResult.Messages.Should().HaveCount(1);
            actualResult.Messages.First().ShortMessage.Should()
                .Be("User does not have permission to export in the source workspace.");
        }

        [Test]
        public async Task ExecuteAllowEditDocumentPermissionTest()
        {
            // Arrange
            Mock<IPermissionsCheckConfiguration> configuration = ConfigurationSet();

            Mock<IPermissionManager> permissionManager = ArrangeSet();

            var permissionToEdit = new List<PermissionValue> { new PermissionValue { PermissionID = _EDIT_DOCUMENT_PERMISSION_ID, Selected = false } };
            permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _EDIT_DOCUMENT_PERMISSION_ID))))
                .ReturnsAsync(permissionToEdit);

            // Act
            ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object).ConfigureAwait(false);

            // Assert
            actualResult.IsValid.Should().BeFalse();
            actualResult.Messages.Should().HaveCount(1);
            actualResult.Messages.First().ShortMessage.Should()
                .Be("User does not have permission to edit Documents in this workspace.");
        }

        [Test]
        public async Task ExecutePermissionValueListEmptyTest()
        {
            // Arrange
            Mock<IPermissionsCheckConfiguration> configuration = ConfigurationSet();

            var permissionManager = new Mock<IPermissionManager>();
            _serviceFactoryForUser.Setup(x => x.CreateProxyAsync<IPermissionManager>()).ReturnsAsync(permissionManager.Object);

            var permissionValuesDefault = new List<PermissionValue>();
            permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>(), It.IsAny<int>())).ReturnsAsync(permissionValuesDefault);
            permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>())).ReturnsAsync(permissionValuesDefault);

            permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _ALLOW_EXPORT_PERMISSION_ID))))
                .ReturnsAsync(permissionValuesDefault);

            permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _EDIT_DOCUMENT_PERMISSION_ID))))
                .ReturnsAsync(permissionValuesDefault);

            // Act
            ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object).ConfigureAwait(false);

            // Assert
            actualResult.IsValid.Should().BeFalse();
            actualResult.Messages.Should().HaveCount(_EXPECTED_VALUE_FOR_ALL_FAILED_VALIDATE);
            actualResult.Messages.First().ShortMessage.Should()
                .Be("User does not have permission to access the source workspace.");
        }

        private Mock<IPermissionManager> ArrangeSet()
        {
            var permissionManager = new Mock<IPermissionManager>();
            _serviceFactoryForUser.Setup(x => x.CreateProxyAsync<IPermissionManager>()).ReturnsAsync(permissionManager.Object);

            var permissionValuesDefault = new List<PermissionValue> { new PermissionValue { Selected = true } };
            permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>(), It.IsAny<int>())).ReturnsAsync(permissionValuesDefault);
            permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>())).ReturnsAsync(permissionValuesDefault);

            var permissionToExport = new List<PermissionValue> { new PermissionValue { PermissionID = _ALLOW_EXPORT_PERMISSION_ID, Selected = true } };
            permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _ALLOW_EXPORT_PERMISSION_ID))))
                .ReturnsAsync(permissionToExport);

            var permissionToEdit = new List<PermissionValue> { new PermissionValue { PermissionID = _EDIT_DOCUMENT_PERMISSION_ID, Selected = true } };
            permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _EDIT_DOCUMENT_PERMISSION_ID))))
                .ReturnsAsync(permissionToEdit);

            return permissionManager;
        }

        private Mock<IPermissionsCheckConfiguration> ConfigurationSet()
        {
            Mock<IPermissionsCheckConfiguration> configuration = new Mock<IPermissionsCheckConfiguration>();
            configuration.Setup(x => x.SourceWorkspaceArtifactId).Returns(_TEST_WORKSPACE_ARTIFACT_ID);
            configuration.SetupGet(x => x.JobHistoryObjectTypeGuid).Returns(_jobHistory);

            return configuration;
        }
    }
}
