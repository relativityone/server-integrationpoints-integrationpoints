using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using Relativity.Services.Objects.DataContracts;
using Relativity.Services.Permission;
using Relativity.Sync.Configuration;
using Relativity.Sync.Executors;
using Relativity.Sync.Executors.PermissionCheck.DocumentPermissionChecks;
using Relativity.Sync.Executors.Validation;
using Relativity.Sync.KeplerFactory;
using Relativity.Sync.Logging;

namespace Relativity.Sync.Tests.Unit.Executors.PermissionCheck
{
	[TestFixture]
	public class DestinationDocumentPermissionCheckTests
	{
		private DestinationDocumentPermissionCheck _sut;
		private Mock<ISyncObjectTypeManager> _syncObjectTypeManagerFake;
		private Mock<IDestinationServiceFactoryForUser> _destinationServiceFactoryFake;

		private const string _SOURCE_WORKSPACE_OBJECT_TYPE_NAME = "Relativity Source Case";
		private const string _SOURCE_JOB_OBJECT_TYPE_NAME = "Relativity Source Job";

		private const int _ALLOW_IMPORT_PERMISSION_ID = 158; // 158 is the artifact id of the "Allow Import" permission
		private const int _ARTIFACT_TYPE_DOCUMENT = 10;
		private const int _ARTIFACT_TYPE_FOLDER = 9;
		private const int _ARTIFACT_TYPE_SEARCH = 15;
		private const int _EXPECTED_VALUE_FOR_DOCUMENT = 1;
		private const int _EXPECTED_VALUE_FOR_ALL_FAILED_VALIDATE = 8;
		private const int _TEST_WORKSPACE_ARTIFACT_ID = 20489;
		private const int _TEST_FOLDER_ARTIFACT_ID = 20476;
		private const int _SOURCE_CASE_OBJECT_TYPE_ARTIFACT_TYPE_ID = 222;
		private const int _SOURCE_JOB_OBJECT_TYPE_ARTIFACT_TYPE_ID = 444;

		[SetUp]
		public void SetUp()
		{
			_syncObjectTypeManagerFake = new Mock<ISyncObjectTypeManager>();
			const int sourceCaseObjectTypeArtifactId = 111;
			const int sourceJobObjectTypeArtifactId = 333;

			_syncObjectTypeManagerFake
				.Setup(x => x.QueryObjectTypeByNameAsync(It.IsAny<int>(),
					It.Is<string>(name => name == _SOURCE_WORKSPACE_OBJECT_TYPE_NAME))).ReturnsAsync(new QueryResult()
				{
					Objects = new List<RelativityObject>()
					{
						new RelativityObject()
						{
							ArtifactID = sourceCaseObjectTypeArtifactId
						}
					}
				});
			_syncObjectTypeManagerFake
				.Setup(x => x.QueryObjectTypeByNameAsync(It.IsAny<int>(),
					It.Is<string>(name => name == _SOURCE_JOB_OBJECT_TYPE_NAME))).ReturnsAsync(new QueryResult()
				{
					Objects = new List<RelativityObject>()
					{
						new RelativityObject()
						{
							ArtifactID = sourceJobObjectTypeArtifactId
						}
					}
				});
			_syncObjectTypeManagerFake
				.Setup(x => x.GetObjectTypeArtifactTypeIdAsync(It.IsAny<int>(),
					It.Is<int>(artifactID => artifactID == sourceCaseObjectTypeArtifactId)))
				.ReturnsAsync( _SOURCE_CASE_OBJECT_TYPE_ARTIFACT_TYPE_ID);
			_syncObjectTypeManagerFake
				.Setup(x => x.GetObjectTypeArtifactTypeIdAsync(It.IsAny<int>(),
					It.Is<int>(artifactID => artifactID == sourceJobObjectTypeArtifactId)))
				.ReturnsAsync(_SOURCE_JOB_OBJECT_TYPE_ARTIFACT_TYPE_ID);


			_destinationServiceFactoryFake = new Mock<IDestinationServiceFactoryForUser>();
			_sut = new DestinationDocumentPermissionCheck(_destinationServiceFactoryFake.Object,
				_syncObjectTypeManagerFake.Object, new EmptyLogger());
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
				(y => y.Any(z => z.ArtifactType.ID == _ARTIFACT_TYPE_DOCUMENT)))).Throws<SyncException>();

			// Act
			ValidationResult actualResult =
				await _sut.ValidateAsync(configuration.Object).ConfigureAwait(false);

			//Assert
			actualResult.IsValid.Should().BeFalse();
			actualResult.Messages.Should().HaveCount(1);
			actualResult.Messages.First().ShortMessage.Should().Be(
				"User does not have permissions to view, edit, and add Documents in the destination workspace.");
		}

		[Test]
		public async Task Validate_ShouldFail_WhenUserDoesNotHavePermissionToCreateSavedSearchAndSavedSearchShouldBeCreated()
		{
			// Arrange
			Mock<IPermissionsCheckConfiguration> configuration = SetupConfiguration(createSavedSearchForTags: true);

			Mock<IPermissionManager> permissionManager = SetupPermissions();

			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>
				(y => y.Any(z => z.ArtifactType.ID == _ARTIFACT_TYPE_SEARCH)))).Throws<SyncException>();

			// Act
			ValidationResult actualResult =
				await _sut.ValidateAsync(configuration.Object).ConfigureAwait(false);

			//Assert
			actualResult.IsValid.Should().BeFalse();
			actualResult.Messages.Should().HaveCount(1);
			actualResult.Messages.First().ShortMessage.Should().Be(
				"User does not have permission to create saved searches in the destination workspace.");
		}

		[Test]
		public async Task Validate_ShouldPass_WhenUserDoesNotHavePermissionToCreateSavedSearchAndSavedSearchShouldNotBeCreated()
		{
			// Arrange
			Mock<IPermissionsCheckConfiguration> configuration = SetupConfiguration(createSavedSearchForTags: false);

			Mock<IPermissionManager> permissionManager = SetupPermissions();

			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>
				(y => y.Any(z => z.ArtifactType.ID == _ARTIFACT_TYPE_SEARCH)))).Throws<SyncException>();

			// Act
			ValidationResult actualResult =
				await _sut.ValidateAsync(configuration.Object).ConfigureAwait(false);

			//Assert
			actualResult.IsValid.Should().BeTrue();
			actualResult.Messages.Should().HaveCount(0);
		}

		[Test]
		public async Task Validate_ShouldFail_WhenUserDoesNotHavePermissionToAccessDestinationWorkspace()
		{
			// Arrange
			Mock<IPermissionsCheckConfiguration> configuration = SetupConfiguration();

			Mock<IPermissionManager> permissionManager = SetupPermissions();

			permissionManager
				.Setup(x => x.GetPermissionSelectedAsync(
					It.IsAny<int>(),
					It.Is<List<PermissionRef>>(permissionRefs => permissionRefs.Any(z => z.ArtifactType.ID == _ARTIFACT_TYPE_DOCUMENT)),
					It.IsAny<int>()))
					.Throws<SyncException>();

			// Act
			ValidationResult actualResult =
				await _sut.ValidateAsync(configuration.Object).ConfigureAwait(false);

			//Assert
			actualResult.IsValid.Should().BeFalse();
			actualResult.Messages.Should().HaveCount(_EXPECTED_VALUE_FOR_DOCUMENT);
			actualResult.Messages.First().ShortMessage.Should().Be(
				"User does not have permission to access the folder in the destination workspace or the folder does not exist.");
			actualResult.Messages.First().ErrorCode.Should().Be("20.009");
		}

		[Test]
		public async Task Validate_ShouldFail_WhenUserDoesNotHavePermissionToAccessDestinationFolder()
		{
			// Arrange
			Mock<IPermissionsCheckConfiguration> configuration = SetupConfiguration();

			Mock<IPermissionManager> permissionManager = SetupPermissions();

			permissionManager.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>
				(y => y.Any(z => z.ArtifactType.ID == _ARTIFACT_TYPE_FOLDER)), It.IsAny<int>())).Throws<SyncException>();

			// Act
			ValidationResult actualResult =
				await _sut.ValidateAsync(configuration.Object).ConfigureAwait(false);

			//Assert
			actualResult.IsValid.Should().BeFalse();
			actualResult.Messages.Should().HaveCount(1);
			actualResult.Messages.First().ShortMessage.Should().Be(
				"User does not have permission to access the folder in the destination workspace or the folder does not exist.");
			actualResult.Messages.First().ErrorCode.Should().Be("20.009");
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

		[Test]
		public async Task Validate_ShouldFail_WhenUserDoesNotHavePermissionsToCreateTagsInDestination()
		{
			// Arrange
			Mock<IPermissionsCheckConfiguration> configuration = SetupConfiguration();

			Mock<IPermissionManager> permissionManager = SetupPermissions();

			var permission = new List<PermissionValue>
			{
				new PermissionValue
				{
					Selected = false
				}
			};
			permissionManager
				.Setup(x => x.GetPermissionSelectedAsync(It.IsAny<int>(), It.Is<List<PermissionRef>>(permissionRefs => permissionRefs.Any(permissionRef =>
					permissionRef.ArtifactType.ID == _SOURCE_CASE_OBJECT_TYPE_ARTIFACT_TYPE_ID ||
					permissionRef.ArtifactType.ID == _SOURCE_JOB_OBJECT_TYPE_ARTIFACT_TYPE_ID))))
				.ReturnsAsync(permission);

			// Act
			ValidationResult actualResult = await _sut.ValidateAsync(configuration.Object).ConfigureAwait(false);

			// Assert
			AssertInsufficientPermissionsToCreateTagInDestination(actualResult);
		}


		private static void AssertInsufficientPermissionsToCreateTagInDestination(ValidationResult actualResult)
		{
			actualResult.IsValid.Should().BeFalse();
			const int expectedNumberOfErrors = 2;
			actualResult.Messages.Should().HaveCount(expectedNumberOfErrors);
			actualResult.Messages
				.All(x => x.ShortMessage.StartsWith("User does not have permissions to create tag", StringComparison.InvariantCulture))
				.Should().BeTrue();
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

			return permissionManager;
		}

		private static Mock<IPermissionsCheckConfiguration> SetupConfiguration(bool createSavedSearchForTags = true)
		{
			Mock<IPermissionsCheckConfiguration> configuration = new Mock<IPermissionsCheckConfiguration>();
			configuration.Setup(x => x.DestinationWorkspaceArtifactId).Returns(_TEST_WORKSPACE_ARTIFACT_ID);
			configuration.Setup(x => x.DestinationFolderArtifactId).Returns(_TEST_FOLDER_ARTIFACT_ID);
			configuration.Setup(x => x.CreateSavedSearchForTags).Returns(createSavedSearchForTags);

			return configuration;
		}
	}
}