using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts.Interfaces;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using ArtifactType = Relativity.ArtifactType;

namespace kCura.IntegrationPoints.Core.Tests.Validation.RelativityProviderValidator.Parts
{
    [TestFixture, Category("Unit")]
    public class RelativityProviderDestinationFolderPermissionValidatorTests
    {
        private IPermissionManager _permissionManager;
        private IRelativityProviderDestinationFolderPermissionValidator _sut;
        private const int DESTINATION_WORKSPACE_ID = 54227;
        private const int DESTINATION_FOLDER_ID = 36492;

        [SetUp]
        public void SetUp()
        {
            _permissionManager = Substitute.For<IPermissionManager>();

            _permissionManager.UserHasArtifactInstancePermission(Arg.Any<int>(), Arg.Any<int>(), Arg.Any<int>(),
                Arg.Any<ArtifactPermission>()).Returns(true); // by default it will return true

            _sut = new RelativityProviderDestinationFolderPermissionValidator(DESTINATION_WORKSPACE_ID, _permissionManager);
        }

        [Test]
        public void ItShouldPass_WhenHasAllPermissions([Values(true, false)] bool useFolderPathInfo, [Values(true, false)] bool moveExistingDocuments)
        {
            // act
            ValidationResult result = _sut.Validate(DESTINATION_FOLDER_ID, useFolderPathInfo, moveExistingDocuments);

            // assert
            Assert.IsTrue(result.IsValid);
        }


        [TestCase(ArtifactType.Folder, ArtifactPermission.Create)]
        [TestCase(ArtifactType.Document, ArtifactPermission.Create)]
        [TestCase(ArtifactType.Document, ArtifactPermission.Delete)]
        public void ItShouldFail_WhenOnePermissionIsMissing(ArtifactType artifactType, ArtifactPermission permission)
        {
            // arrange
            SetPermissionToFalse(artifactType, permission);

            // act
            ValidationResult result = _sut.Validate(DESTINATION_FOLDER_ID, useFolderPathInfo: true, moveExistingDocuments: true);

            // assert
            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void ItShouldFailWhen_NoSubfolderCreationPermission_AndUsingFolderPathInfo()
        {
            // arrange
            SetPermissionToFalse(ArtifactType.Folder, ArtifactPermission.Create);

            // act
            ValidationResult result = _sut.Validate(DESTINATION_FOLDER_ID, useFolderPathInfo: true, moveExistingDocuments: false);

            // assert
            Assert.IsFalse(result.IsValid);
        }

        [Test]
        public void ItShouldPassWhen_NoSubfolderCreationPermission_And_NotUsingFolderPathInfo()
        {
            // arrange
            SetPermissionToFalse(ArtifactType.Folder, ArtifactPermission.Create);

            // act
            ValidationResult result = _sut.Validate(DESTINATION_FOLDER_ID, useFolderPathInfo: false, moveExistingDocuments: false);

            // assert
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ItShouldPassWhen_NoDocumentDeletePermission_And_NotMovingExistingDocuments()
        {
            // arrange
            SetPermissionToFalse(ArtifactType.Document, ArtifactPermission.Delete);

            // act
            ValidationResult result = _sut.Validate(DESTINATION_FOLDER_ID, useFolderPathInfo: true, moveExistingDocuments: false);

            // assert
            Assert.IsTrue(result.IsValid);
        }

        [Test]
        public void ItShouldFailWhen_NoDocumentCreationPermission()
        {
            // arrange
            SetPermissionToFalse(ArtifactType.Document, ArtifactPermission.Create);

            // act
            ValidationResult result = _sut.Validate(DESTINATION_FOLDER_ID, useFolderPathInfo: false, moveExistingDocuments: false);

            // assert
            Assert.IsFalse(result.IsValid);
        }

        private void SetPermissionToFalse(ArtifactType artifactType, ArtifactPermission permission)
        {
            _permissionManager.UserHasArtifactInstancePermission(DESTINATION_WORKSPACE_ID, (int)artifactType,
                DESTINATION_FOLDER_ID, permission).Returns(false);
        }
    }
}
