using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Permission;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors.PermissionCheck;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;

namespace Relativity.Sync.Tests.Unit.Executors.PermissionCheck
{
	[TestFixture]
	public class DestinationPermissionCheckTests
	{
		private DestinationPermissionCheck _instance;
		private Mock<ISyncLog> _logger;
		private Mock<IDestinationServiceFactoryForUser> _destinationServiceFactory;

		private const int _ALLOW_IMPORT_PERMISSION_ID = 158; // 158 is the artifact id of the "Allow Import" permission
		private const int _ARTIFACT_TYPE_DOCUMENT = 10;
		private const int _ARTIFACT_TYPE_FOLDER = 9;
		private const int _ARTIFACT_TYPE_SEARCH = 15;
		private const int _EXPECTED_VALUE_FOR_DOCUMENT = 2;
		private const int _ARTIFACT_OBJECT_TYPE = 25;
		private const int _EXPECTED_VALUE_FOR_ALL_FAILED_VALIDATE = 8;
		private const int _TEST_WORKSPACE_ARTIFACT_ID = 20489;
		private const int _TEST_FOLDER_ARTIFACT_ID = 20476;

		[SetUp]
		public void SetUp()
		{
			_logger = new Mock<ISyncLog>();
			_destinationServiceFactory = new Mock<IDestinationServiceFactoryForUser>();
			_instance = new DestinationPermissionCheck(_destinationServiceFactory.Object, _logger.Object);
		}

		[Test]
		public async Task ValidateAsyncGoldFlowTest()
		{
			// Arrange
			Mock<IPermissionsCheckConfiguration> configuration = ConfigurationSet();

			Mock<IPermissionManager> permissionManager = ArrangeSet();

			//Act
			ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object).ConfigureAwait(false);

			//Assert
			actualResult.IsValid.Should().BeTrue();
			actualResult.Messages.Should().HaveCount(0);
		}

		[Test]
		public async Task UserShouldNotHavePermissionToAddObjectType()
		{
			// Arrange
			Mock<IPermissionsCheckConfiguration> configuration = ConfigurationSet();

			Mock<IPermissionManager> permissionManager = ArrangeSet();

			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(),
				It.Is<List<PermissionRef>>(y => y.Any(z => z.ArtifactType.ID == _ARTIFACT_OBJECT_TYPE)))).Throws<SyncException>();

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object).ConfigureAwait(false);

			// Assert
			actualResult.IsValid.Should().BeFalse();
			actualResult.Messages.Should().HaveCount(1);
			actualResult.Messages.First().ShortMessage.Should()
				.Be("User does not have permission to add object types in the destination workspace.");
		}

		[Test]
		public async Task UserShouldNotHavePermissionToAccessDestinationWorkspaceTest()
		{
			// Arrange
			Mock<IPermissionsCheckConfiguration> configuration = ConfigurationSet();

			Mock <IPermissionManager> permissionManager = ArrangeSet();

			permissionManager
				.Setup(x => x.GetPermissionSelectedAsync(-1, It.IsAny<List<PermissionRef>>(), It.IsAny<int>()))
				.Throws<SyncException>();

			// Act
			ValidationResult actualResult =
				await _instance.ValidateAsync(configuration.Object).ConfigureAwait(false);

			//Assert
			actualResult.IsValid.Should().BeFalse();
			actualResult.Messages.Should().HaveCount(1);
			actualResult.Messages.First().ShortMessage.Should().Be(
				"User does not have sufficient permissions to access destination workspace. Contact your system administrator.");
			actualResult.Messages.First().ErrorCode.Should().Be("20.001");
		}

		[Test]
		public async Task UserShouldNotHavePermissionToImportInTheDestinationWorkspaceTest()
		{
			// Arrange
			Mock<IPermissionsCheckConfiguration> configuration = ConfigurationSet();

			Mock<IPermissionManager> permissionManager = ArrangeSet();

			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>
					( y => y.Any( z => z.PermissionID == _ALLOW_IMPORT_PERMISSION_ID)))).Throws<SyncException>();

			// Act
			ValidationResult actualResult =
				await _instance.ValidateAsync(configuration.Object).ConfigureAwait(false);

			//Assert
			actualResult.IsValid.Should().BeFalse();
			actualResult.Messages.Should().HaveCount(1);
			actualResult.Messages.First().ShortMessage.Should().Be(
				"User does not have permission to import in the destination workspace.");
		}

		[Test]
		public async Task UserShouldNotHaveAllRequiredRdoPermissionTest()
		{
			// Arrange
			Mock<IPermissionsCheckConfiguration> configuration = ConfigurationSet();

			Mock<IPermissionManager> permissionManager = ArrangeSet();

			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>
				(y => y.Any(z => z.ArtifactType.ID == _ARTIFACT_TYPE_DOCUMENT)))).Throws<SyncException>();

			// Act
			ValidationResult actualResult =
				await _instance.ValidateAsync(configuration.Object).ConfigureAwait(false);

			//Assert
			actualResult.IsValid.Should().BeFalse();
			actualResult.Messages.Should().HaveCount(1);
			actualResult.Messages.First().ShortMessage.Should().Be(
				"User does not have permissions to view, edit, and add Documents in the destination workspace.");
		}

		[Test]
		public async Task UserShouldNotPermissionToCreateSavedSearchTest()
		{
			// Arrange
			Mock<IPermissionsCheckConfiguration> configuration = ConfigurationSet();

			Mock<IPermissionManager> permissionManager = ArrangeSet();

			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>
				(y => y.Any(z => z.ArtifactType.ID == _ARTIFACT_TYPE_SEARCH)))).Throws<SyncException>();

			// Act
			ValidationResult actualResult =
				await _instance.ValidateAsync(configuration.Object).ConfigureAwait(false);

			//Assert
			actualResult.IsValid.Should().BeFalse();
			actualResult.Messages.Should().HaveCount(1);
			actualResult.Messages.First().ShortMessage.Should().Be(
				"User does not have permission to create saved searches in the destination workspace.");
		}

		[Test]
		public async Task UserShouldNotPermissionToAccessDestinationWorkspaceTest()
		{
			// Arrange
			Mock<IPermissionsCheckConfiguration> configuration = ConfigurationSet();

			Mock<IPermissionManager> permissionManager = ArrangeSet();

			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>
				(y => y.Any(z => z.ArtifactType.ID == _ARTIFACT_TYPE_DOCUMENT)),It.IsAny<int>())).Throws<SyncException>();

			// Act
			ValidationResult actualResult =
				await _instance.ValidateAsync(configuration.Object).ConfigureAwait(false);

			//Assert
			actualResult.IsValid.Should().BeFalse();
			actualResult.Messages.Should().HaveCount(_EXPECTED_VALUE_FOR_DOCUMENT);
			actualResult.Messages.First().ShortMessage.Should().Be(
				"User does not have permission to access the folder in the destination workspace or the folder does not exist.");
			actualResult.Messages.First().ErrorCode.Should().Be("20.009");
		}

		[Test]
		public async Task UserShouldNotPermissionToAccessDestinationWorkspaceForFolderTest()
		{
			// Arrange
			Mock<IPermissionsCheckConfiguration> configuration = ConfigurationSet();

			Mock<IPermissionManager> permissionManager = ArrangeSet();

			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>
				(y => y.Any(z => z.ArtifactType.ID == _ARTIFACT_TYPE_FOLDER)), It.IsAny<int>())).Throws<SyncException>();

			// Act
			ValidationResult actualResult =
				await _instance.ValidateAsync(configuration.Object).ConfigureAwait(false);

			//Assert
			actualResult.IsValid.Should().BeFalse();
			actualResult.Messages.Should().HaveCount(1);
			actualResult.Messages.First().ShortMessage.Should().Be(
				"User does not have permission to access the folder in the destination workspace or the folder does not exist.");
			actualResult.Messages.First().ErrorCode.Should().Be("20.009");
		}

		[Test]
		public async Task ExecuteAllowImportPermissionTest()
		{
			// Arrange
			Mock<IPermissionsCheckConfiguration> configuration = ConfigurationSet();

			Mock<IPermissionManager> permissionManager = ArrangeSet();

			var permissionToEdit = new List<PermissionValue> { new PermissionValue { PermissionID = _ALLOW_IMPORT_PERMISSION_ID, Selected = false } };
			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _ALLOW_IMPORT_PERMISSION_ID))))
				.ReturnsAsync(permissionToEdit);

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object).ConfigureAwait(false);

			// Assert
			actualResult.IsValid.Should().BeFalse();
			actualResult.Messages.Should().HaveCount(1);
			actualResult.Messages.First().ShortMessage.Should()
				.Be("User does not have permission to import in the destination workspace.");
		}

		[Test]
		public async Task ExecutePermissionValueListEmptyTest()
		{
			// Arrange
			Mock<IPermissionsCheckConfiguration> configuration = ConfigurationSet();

			var permissionManager = new Mock<IPermissionManager>();
			_destinationServiceFactory.Setup(x => x.CreateProxyAsync<IPermissionManager>()).ReturnsAsync(permissionManager.Object);

			var permissionValuesDefault = new List<PermissionValue>();
			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>(),
				It.IsAny<int>())).ReturnsAsync(permissionValuesDefault);

			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>())).ReturnsAsync(permissionValuesDefault);

			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(),
				It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _ALLOW_IMPORT_PERMISSION_ID)))).ReturnsAsync(permissionValuesDefault);

			// Act
			ValidationResult actualResult = await _instance.ValidateAsync(configuration.Object).ConfigureAwait(false);

			// Assert
			actualResult.IsValid.Should().BeFalse();
			actualResult.Messages.Should().HaveCount(_EXPECTED_VALUE_FOR_ALL_FAILED_VALIDATE);
			actualResult.Messages.First().ShortMessage.Should()
				.Be("User does not have sufficient permissions to access destination workspace. Contact your system administrator.");
		}

		private Mock<IPermissionManager> ArrangeSet()
		{
			var permissionManager = new Mock<IPermissionManager>();
			_destinationServiceFactory.Setup(x => x.CreateProxyAsync<IPermissionManager>())
				.ReturnsAsync(permissionManager.Object);

			var permissionValueDefault = new List<PermissionValue> { new PermissionValue { Selected = true } };
			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>(),
				It.IsAny<int>())).ReturnsAsync(permissionValueDefault);

			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.IsAny<List<PermissionRef>>())).ReturnsAsync(permissionValueDefault);

			var permissionToExport = new List<PermissionValue> { new PermissionValue { Selected = true, PermissionID = _ALLOW_IMPORT_PERMISSION_ID } };
			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(),
				It.Is<List<PermissionRef>>(y => y.Any(z => z.PermissionID == _ALLOW_IMPORT_PERMISSION_ID)))).ReturnsAsync(permissionToExport);

			return permissionManager;
		}

		private Mock<IPermissionsCheckConfiguration> ConfigurationSet()
		{
			Mock<IPermissionsCheckConfiguration> configuration = new Mock<IPermissionsCheckConfiguration>();
			configuration.Setup(x => x.DestinationWorkspaceArtifactId).Returns(_TEST_WORKSPACE_ARTIFACT_ID);
			configuration.Setup(x => x.DestinationFolderArtifactId).Returns(_TEST_FOLDER_ARTIFACT_ID);
			return configuration;
		}
	}
}