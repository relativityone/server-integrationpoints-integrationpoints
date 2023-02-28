using System.Collections.Generic;
using System.Linq;
using kCura.IntegrationPoints.Core.Managers;
using kCura.IntegrationPoints.Core.Validation.RelativityProviderValidator.Parts;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Factories;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;
using Relativity;

namespace kCura.IntegrationPoints.Core.Tests.Validation.RelativityProviderValidator.Parts
{
    [TestFixture, Category("Unit")]
    public class RelativityProviderDestinationWorkspacePermissionValidatorTests
    {
        private IPermissionManager _permissionManager;
        private IRepositoryFactory _repositoryFactory;
        private const int _WORKSPACE_ID = 4;
        private const int _OBJECT_TYPE_ID = 7;

        [SetUp]
        public void SetUp()
        {
            _permissionManager = Substitute.For<IPermissionManager>();
            _permissionManager.UserHasPermissionToAccessWorkspace(_WORKSPACE_ID).Returns(true);
            _permissionManager.UserCanImport(_WORKSPACE_ID).Returns(true);
            _permissionManager.UserHasArtifactTypePermissions(_WORKSPACE_ID, _OBJECT_TYPE_ID,
                Arg.Any<IEnumerable<ArtifactPermission>>()).Returns(true);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ItShouldValidateDestinationPermissionAccessibility(bool isDestinationWorkspaceAccessible)
        {
            _permissionManager.UserHasPermissionToAccessWorkspace(_WORKSPACE_ID).Returns(isDestinationWorkspaceAccessible);

            var sut = new RelativityProviderDestinationWorkspacePermissionValidator(_permissionManager, _repositoryFactory);

            // act
            ValidationResult result = sut.Validate(_WORKSPACE_ID, _OBJECT_TYPE_ID, false);

            // assert
            _permissionManager.Received().UserHasPermissionToAccessWorkspace(_WORKSPACE_ID);
            Assert.AreEqual(isDestinationWorkspaceAccessible, result.IsValid);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ItShouldValidateImportPermission_WhenDestinationWorkspaceIsAccessible(bool canImport)
        {
            _permissionManager.UserCanImport(_WORKSPACE_ID).Returns(canImport);

            var sut = new RelativityProviderDestinationWorkspacePermissionValidator(_permissionManager, _repositoryFactory);

            // act
            ValidationResult result = sut.Validate(_WORKSPACE_ID, _OBJECT_TYPE_ID, false);

            // assert
            _permissionManager.Received().UserCanImport(_WORKSPACE_ID);
            Assert.AreEqual(canImport, result.IsValid);
        }

        [TestCase(true)]
        [TestCase(false)]
        public void ItShouldNotValidateImportPermission_WhenDestinationWorkspaceIsInaccessible(bool createSavedSearch)
        {
            _permissionManager.UserHasPermissionToAccessWorkspace(_WORKSPACE_ID).Returns(false);

            var sut = new RelativityProviderDestinationWorkspacePermissionValidator(_permissionManager, _repositoryFactory);

            // act
            sut.Validate(_WORKSPACE_ID, _OBJECT_TYPE_ID, createSavedSearch);

            // assert
            _permissionManager.DidNotReceiveWithAnyArgs().UserCanImport(_WORKSPACE_ID);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ItShouldValidateArtifactTypePermission_WhenDestinationWorkspaceIsAccessible(bool accessToArtifactType)
        {
            _permissionManager.UserHasArtifactTypePermissions(_WORKSPACE_ID, _OBJECT_TYPE_ID, Arg.Any<ArtifactPermission[]>()).Returns(accessToArtifactType);

            var sut = new RelativityProviderDestinationWorkspacePermissionValidator(_permissionManager, _repositoryFactory);

            // act
            ValidationResult result = sut.Validate(_WORKSPACE_ID, _OBJECT_TYPE_ID, false);

            // assert
            ArtifactPermission[] expectedPermissions = {ArtifactPermission.View, ArtifactPermission.Create, ArtifactPermission.Edit};
            foreach (ArtifactPermission expectedPermission in expectedPermissions)
            {
                _permissionManager.Received().UserHasArtifactTypePermissions(_WORKSPACE_ID, _OBJECT_TYPE_ID,
                    Arg.Is<ArtifactPermission[]>(x => x.Contains(expectedPermission)));
            }
            Assert.AreEqual(accessToArtifactType, result.IsValid);
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ItShouldNotValidateArtifactTypePermission_WhenDestinationWorkspaceIsInaccessible(bool createSavedSearch)
        {
            _permissionManager.UserHasPermissionToAccessWorkspace(_WORKSPACE_ID).Returns(false);

            var sut = new RelativityProviderDestinationWorkspacePermissionValidator(_permissionManager, _repositoryFactory);

            // act
            sut.Validate(_WORKSPACE_ID, _OBJECT_TYPE_ID, createSavedSearch);

            // assert
            _permissionManager.DidNotReceiveWithAnyArgs().UserHasArtifactTypePermissions(_WORKSPACE_ID, _OBJECT_TYPE_ID, Arg.Any<ArtifactPermission[]>());
        }

        [TestCase(false)]
        [TestCase(true)]
        public void ItShouldNotValidateSavedSearchPermission_WhenDestinationWorkspaceIsInaccessible(bool createSavedSearch)
        {
            _permissionManager.UserHasPermissionToAccessWorkspace(_WORKSPACE_ID).Returns(false);

            var validator = new RelativityProviderDestinationWorkspacePermissionValidator(_permissionManager, _repositoryFactory);

            // act
            validator.Validate(_WORKSPACE_ID, _OBJECT_TYPE_ID, createSavedSearch);

            // assert
            _permissionManager.DidNotReceiveWithAnyArgs().UserHasArtifactTypePermission(_WORKSPACE_ID, (int) ArtifactType.Search, ArtifactPermission.Create);
        }

        [Test]
        public void ItShouldValidateSavedSearchPermission_WhenDestinationWorkspaceIsAccessible(
            [Values(true, false)] bool createSavedSearch,
            [Values(true, false)] bool canCreateSavedSearch)
        {
            _permissionManager.UserHasArtifactTypePermission(_WORKSPACE_ID, (int)ArtifactType.Search, ArtifactPermission.Create).Returns(canCreateSavedSearch);

            var validator = new RelativityProviderDestinationWorkspacePermissionValidator(_permissionManager, _repositoryFactory);

            // act
            ValidationResult result = validator.Validate(_WORKSPACE_ID, _OBJECT_TYPE_ID, createSavedSearch);

            // assert
            _permissionManager.Received(createSavedSearch ? 1 : 0).UserHasArtifactTypePermission(_WORKSPACE_ID, (int)ArtifactType.Search, ArtifactPermission.Create);
            Assert.AreEqual(!(createSavedSearch && !canCreateSavedSearch), result.IsValid);
        }
    }
}
