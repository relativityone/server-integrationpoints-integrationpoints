using System;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Domain.Models;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Validation
{
    [TestFixture, Category("Unit")]
    class StopJobPermissionValidatorTests : PermissionValidatorTestsBase
    {
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void ValidateTest(bool canEditIntegrationPoint, bool canEditJobHistory)
        {
            // arrange
            _sourcePermissionRepository.UserHasArtifactInstancePermission(_validationModel.ObjectTypeGuid, INTEGRATION_POINT_ID, ArtifactPermission.Edit).Returns(canEditIntegrationPoint);
            _sourcePermissionRepository.UserHasArtifactTypePermission(Arg.Is<Guid>(guid => guid == new Guid(ObjectTypeGuids.JobHistory)), ArtifactPermission.Edit).Returns(canEditJobHistory);

            var stopJobPermissionValidator = new StopJobPermissionValidator(_repositoryFactory, _serializer, ServiceContextHelper);

            // act
            ValidationResult validationResult = stopJobPermissionValidator.Validate(_validationModel);

            // assert
            validationResult.Check(canEditIntegrationPoint && canEditJobHistory);

            _sourcePermissionRepository.Received(1).UserHasArtifactInstancePermission(_validationModel.ObjectTypeGuid, INTEGRATION_POINT_ID, ArtifactPermission.Edit);
            _sourcePermissionRepository.Received(1)
                .UserHasArtifactTypePermission(Arg.Is<Guid>(guid => guid == new Guid(ObjectTypeGuids.JobHistory)),
                    ArtifactPermission.Edit);
        }
    }
}
