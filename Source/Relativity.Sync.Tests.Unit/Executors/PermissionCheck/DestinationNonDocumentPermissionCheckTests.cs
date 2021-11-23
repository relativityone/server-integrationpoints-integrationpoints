using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services;
using Relativity.Services.Permission;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.PermissionCheck.NonDocumentPermissionChecks;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Executors.PermissionCheck
{
	[TestFixture]
	public class DestinationNonDocumentPermissionCheckTests
	{
		private DestinationNonDocumentPermissionCheck _sut;
		private Mock<IDestinationServiceFactoryForUser> _destinationServiceFactoryFake;
		
		private const int _RDO_ARTIFACT_TYPE_ID = 420;

		private const int _ALLOW_IMPORT_PERMISSION_ID = 158; // 158 is the artifact id of the "Allow Import" permission
		private const int _EXPECTED_VALUE_FOR_DOCUMENT = 1;
		private const int _EXPECTED_VALUE_FOR_ALL_FAILED_VALIDATE = 3;
		private const int _TEST_WORKSPACE_ARTIFACT_ID = 20489;
		private const int _TEST_FOLDER_ARTIFACT_ID = 20476;

		[SetUp]
		public void SetUp()
		{
			_destinationServiceFactoryFake = new Mock<IDestinationServiceFactoryForUser>();
			_sut = new DestinationNonDocumentPermissionCheck(_destinationServiceFactoryFake.Object, new EmptyLogger());
		}

		[Test]
		public async Task Validate_ShouldPassValidation_WhenUserHasAllPermissions()
		{
			// Arrange
			Mock<IPermissionsCheckConfiguration> configuration = SetupConfiguration();

			Mock<IPermissionManager> permissionManager = SetupPermissions();

			//Act
			ValidationResult actualResult = await _sut.ValidateAsync(configuration.Object).ConfigureAwait(false);

			//Assert
			actualResult.IsValid.Should().BeTrue();
			actualResult.Messages.Should().HaveCount(0);
		}

		[Test]
		public async Task Validate_ShouldFail_WhenUserDoesNotHaveAccessToDestinationWorkspace()
		{
			// Arrange
			Mock<IPermissionsCheckConfiguration> configuration = SetupConfiguration();

			Mock<IPermissionManager> permissionManager = SetupPermissions();

			permissionManager
				.Setup(x => x.GetPermissionSelectedAsync(-1, It.IsAny<List<PermissionRef>>(), It.IsAny<int>()))
				.Throws<SyncException>();

			// Act
			ValidationResult actualResult =
				await _sut.ValidateAsync(configuration.Object).ConfigureAwait(false);

			//Assert
			actualResult.IsValid.Should().BeFalse();
			actualResult.Messages.Should().HaveCount(1);
			actualResult.Messages.First().ShortMessage.Should().Be(
				"User does not have sufficient permissions to access destination workspace. Contact your system administrator.");
			actualResult.Messages.First().ErrorCode.Should().Be("20.001");
		}

		[Test]
		public async Task Validate_ShouldFail_WhenUserDoesNotHavePermissionToImportInTheDestinationWorkspace()
		{
			// Arrange
			Mock<IPermissionsCheckConfiguration> configuration = SetupConfiguration();

			Mock<IPermissionManager> permissionManager = SetupPermissions();

			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>
					(y => y.Any(z => z.PermissionID == _ALLOW_IMPORT_PERMISSION_ID)))).Throws<SyncException>();

			// Act
			ValidationResult actualResult =
				await _sut.ValidateAsync(configuration.Object).ConfigureAwait(false);

			//Assert
			actualResult.IsValid.Should().BeFalse();
			actualResult.Messages.Should().HaveCount(1);
			actualResult.Messages.First().ShortMessage.Should().Be(
				"User does not have permission to import in the destination workspace.");
		}

		[Test]
		public async Task Validate_ShouldFail_WhenUserDoesNotHaveAllRequiredRdoPermissions()
		{
			// Arrange
			Mock<IPermissionsCheckConfiguration> configuration = SetupConfiguration();

			Mock<IPermissionManager> permissionManager = SetupPermissions();

			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>
				(y => y.Any(z => z.ArtifactType.ID == _RDO_ARTIFACT_TYPE_ID)))).Throws<SyncException>();

			// Act
			ValidationResult actualResult =
				await _sut.ValidateAsync(configuration.Object).ConfigureAwait(false);

			//Assert
			actualResult.IsValid.Should().BeFalse();
			actualResult.Messages.Should().HaveCount(1);
			actualResult.Messages.First().ShortMessage.Should().Be(
				$"User does not have permission to add objects of type {_RDO_ARTIFACT_TYPE_ID} in the destination workspace.");
		}

		[TestCase(ImportOverwriteMode.AppendOverlay)]
		[TestCase(ImportOverwriteMode.OverlayOnly)]
		public async Task Validate_ShouldFail_WhenUserDoesNotHavePermissionToEditTransferredObject(ImportOverwriteMode overlayMode)
		{
			// Arrange
			Mock<IPermissionsCheckConfiguration> configuration = SetupConfiguration(overlayMode);

			Mock<IPermissionManager> permissionManager = SetupPermissions();

			permissionManager
				.Setup(x => x.GetPermissionSelectedAsync(
					It.IsAny<int>(),
					It.Is<List<PermissionRef>>(permissionRefs => 
						permissionRefs.Any(z => z.ArtifactType.ID == _RDO_ARTIFACT_TYPE_ID 
						&& z.PermissionType.Equals(PermissionType.Edit)))))
					.Throws<SyncException>();

			// Act
			ValidationResult actualResult =
				await _sut.ValidateAsync(configuration.Object).ConfigureAwait(false);

			//Assert
			actualResult.IsValid.Should().BeFalse();
			actualResult.Messages.Should().HaveCount(_EXPECTED_VALUE_FOR_DOCUMENT);
			actualResult.Messages.First().ShortMessage.Should().Be(
				$"User does not have permission to Edit objects of type {_RDO_ARTIFACT_TYPE_ID} in the destination workspace.");
		}

		[Test]
		public async Task Validate_ShouldFail_WhenUserDoesNotHaveImportPermissionIntoDestinationWorkspace()
		{
			// Arrange
			Mock<IPermissionsCheckConfiguration> configuration = SetupConfiguration();

			Mock<IPermissionManager> permissionManager = SetupPermissions();

			var permissionToEdit = new List<PermissionValue> { new PermissionValue { PermissionID = _ALLOW_IMPORT_PERMISSION_ID, Selected = false } };
			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _ALLOW_IMPORT_PERMISSION_ID))))
				.ReturnsAsync(permissionToEdit);

			// Act
			ValidationResult actualResult = await _sut.ValidateAsync(configuration.Object).ConfigureAwait(false);

			// Assert
			actualResult.IsValid.Should().BeFalse();
			actualResult.Messages.Should().HaveCount(1);
			actualResult.Messages.First().ShortMessage.Should()
				.Be("User does not have permission to import in the destination workspace.");
		}

		[Test]
		public async Task Validate_ShouldFail_WhenUserDoesNotHavePermissionsToAccessDestination()
		{
			// Arrange
			Mock<IPermissionsCheckConfiguration> configuration = SetupConfiguration();

			var permissionManager = new Mock<IPermissionManager>();
			_destinationServiceFactoryFake.Setup(x => x.CreateProxyAsync<IPermissionManager>()).ReturnsAsync(permissionManager.Object);

			var permissionValuesDefault = new List<PermissionValue>();
			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>(),
				It.IsAny<int>())).ReturnsAsync(permissionValuesDefault);

			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>())).ReturnsAsync(permissionValuesDefault);

			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(),
				It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _ALLOW_IMPORT_PERMISSION_ID)))).ReturnsAsync(permissionValuesDefault);

			// Act
			ValidationResult actualResult = await _sut.ValidateAsync(configuration.Object).ConfigureAwait(false);

			// Assert
			actualResult.IsValid.Should().BeFalse();
			actualResult.Messages.Should().HaveCount(_EXPECTED_VALUE_FOR_ALL_FAILED_VALIDATE);
			actualResult.Messages.First().ShortMessage.Should()
				.Be("User does not have sufficient permissions to access destination workspace. Contact your system administrator.");
		}

		private Mock<IPermissionManager> SetupPermissions()
		{
			var permissionManager = new Mock<IPermissionManager>();
			_destinationServiceFactoryFake.Setup(x => x.CreateProxyAsync<IPermissionManager>())
				.ReturnsAsync(permissionManager.Object);

			var permissionValueDefault = new List<PermissionValue> { new PermissionValue { Selected = true } };
			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>(),
				It.IsAny<int>())).ReturnsAsync(permissionValueDefault);

			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>())).ReturnsAsync(permissionValueDefault);

			var permissionToExport = new List<PermissionValue> { new PermissionValue { Selected = true, PermissionID = _ALLOW_IMPORT_PERMISSION_ID } };
			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(),
				It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _ALLOW_IMPORT_PERMISSION_ID)))).ReturnsAsync(permissionToExport);

			var permissionToAdd = new List<PermissionValue> { new PermissionValue { Selected = true, ArtifactType = new ArtifactTypeIdentifier(_RDO_ARTIFACT_TYPE_ID), PermissionType = PermissionType.Add } };
			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(),
				It.Is<List<PermissionRef>>(y => y.Any(z => z.ArtifactType.ID == _RDO_ARTIFACT_TYPE_ID)))).ReturnsAsync(permissionToAdd);
			
			return permissionManager;
		}

		private static Mock<IPermissionsCheckConfiguration> SetupConfiguration(ImportOverwriteMode importMode = ImportOverwriteMode.AppendOnly)
		{
			Mock<IPermissionsCheckConfiguration> configuration = new Mock<IPermissionsCheckConfiguration>();
			configuration.Setup(x => x.DestinationWorkspaceArtifactId).Returns(_TEST_WORKSPACE_ARTIFACT_ID);
			configuration.Setup(x => x.DestinationFolderArtifactId).Returns(_TEST_FOLDER_ARTIFACT_ID);
			configuration.SetupGet(x => x.RdoArtifactTypeId).Returns(_RDO_ARTIFACT_TYPE_ID);
			configuration.SetupGet(x => x.ImportOverwriteMode).Returns(importMode);
			
			return configuration;
		}
	}
}