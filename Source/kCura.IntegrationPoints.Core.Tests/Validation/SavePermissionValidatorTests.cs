using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Validation
{
    [TestFixture, Category("Unit")]
    public class SavePermissionValidatorTests : PermissionValidatorTestsBase
    {
        //CREATE
        [TestCase(true, true, true, true, true)]
        [TestCase(true, false, false, true, true)]
        [TestCase(true, true, true, false, false)]
        //EDIT
        [TestCase(false, true, true, true, true)]
        [TestCase(false, true, true, false, true)]
        [TestCase(false, false, true, true, false)]
        [TestCase(false, true, false, true, false)]
        public void ValidateTest(bool isNew, bool typeEdit, bool instanceEdit, bool typeCreate, bool expected)
        {
            // arrange
            _sourcePermissionRepository.UserHasArtifactTypePermission(_validationModel.ObjectTypeGuid, ArtifactPermission.Edit).Returns(typeEdit);
            _sourcePermissionRepository.UserHasArtifactInstancePermission(_validationModel.ObjectTypeGuid, _validationModel.ArtifactId, ArtifactPermission.Edit).Returns(instanceEdit);
            _sourcePermissionRepository.UserHasArtifactTypePermission(_validationModel.ObjectTypeGuid, ArtifactPermission.Create).Returns(typeCreate);

            var savePermissionValidator = new SavePermissionValidator(_repositoryFactory, _serializer, ServiceContextHelper);

            if (isNew)
            {
                _validationModel.ArtifactId = 0;
            }

            // act
            ValidationResult validationResult = savePermissionValidator.Validate(_validationModel);

            // assert
            if (isNew)
            {
                _sourcePermissionRepository.DidNotReceive().UserHasArtifactTypePermission(_validationModel.ObjectTypeGuid, ArtifactPermission.Edit);
                _sourcePermissionRepository.DidNotReceive().UserHasArtifactInstancePermission(_validationModel.ObjectTypeGuid, _validationModel.ArtifactId, ArtifactPermission.Edit);

                _sourcePermissionRepository.Received(1).UserHasArtifactTypePermission(_validationModel.ObjectTypeGuid, ArtifactPermission.Create);
            }
            else
            {
                _sourcePermissionRepository.Received(1).UserHasArtifactTypePermission(_validationModel.ObjectTypeGuid, ArtifactPermission.Edit);
                _sourcePermissionRepository.Received(1).UserHasArtifactInstancePermission(_validationModel.ObjectTypeGuid, _validationModel.ArtifactId, ArtifactPermission.Edit);

                _sourcePermissionRepository.DidNotReceive().UserHasArtifactTypePermission(_validationModel.ObjectTypeGuid, ArtifactPermission.Create);
            }

            validationResult.Check(expected);
        }
    }
}
