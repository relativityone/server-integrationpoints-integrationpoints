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
    public class PermissionValidatorTests : PermissionValidatorTestsBase
    {
        [Test, Combinatorial]
        public void ValidateTest(
            [Values(true, false)] bool sourceWorkspacePermission,
            [Values(true, false)] bool integrationPointTypeViewPermission,
            [Values(true, false)] bool integrationPointInstanceViewPermission,
            [Values(true, false)] bool jobHistoryAddPermission,
            [Values(true, false)] bool sourceProviderTypeViewPermission,
            [Values(true, false)] bool destinationProviderViewPermission,
            [Values(true, false)] bool sourceProviderInstanceViewPermission)
        {
            _sourcePermissionRepository.UserHasPermissionToAccessWorkspace().Returns(sourceWorkspacePermission);
            _sourcePermissionRepository.UserHasArtifactTypePermission(_validationModel.ObjectTypeGuid, ArtifactPermission.View).Returns(integrationPointTypeViewPermission);
            _sourcePermissionRepository.UserHasArtifactInstancePermission(_validationModel.ObjectTypeGuid, _validationModel.ArtifactId, ArtifactPermission.View).Returns(integrationPointInstanceViewPermission);
            _sourcePermissionRepository.UserHasArtifactTypePermission(
                Arg.Is(new Guid(ObjectTypeGuids.JobHistory)), ArtifactPermission.Create).Returns(jobHistoryAddPermission);

            _sourcePermissionRepository.UserHasArtifactTypePermission(_SOURCE_PROVIDER_GUID, ArtifactPermission.View).Returns(sourceProviderTypeViewPermission);
            _sourcePermissionRepository.UserHasArtifactTypePermission(_DESTINATION_PROVIDER_GUID, ArtifactPermission.View).Returns(destinationProviderViewPermission);
            _sourcePermissionRepository.UserHasArtifactInstancePermission(_SOURCE_PROVIDER_GUID, _validationModel.SourceProviderArtifactId, ArtifactPermission.View).Returns(sourceProviderInstanceViewPermission);

            // ACT
            var permissionValidator = new PermissionValidator(_repositoryFactory, _serializer, ServiceContextHelper);
            ValidationResult validationResult = permissionValidator.Validate(_validationModel);

            // ASSERT    
            bool expected =
                sourceWorkspacePermission &&
                integrationPointTypeViewPermission &&
                integrationPointInstanceViewPermission &&
                jobHistoryAddPermission &&
                sourceProviderTypeViewPermission &&
                destinationProviderViewPermission &&
                sourceProviderInstanceViewPermission;

            validationResult.Check(expected);

            _sourcePermissionRepository.Received(1).UserHasPermissionToAccessWorkspace();
            _sourcePermissionRepository.Received(1).UserHasArtifactTypePermission(_validationModel.ObjectTypeGuid, ArtifactPermission.View);
            _sourcePermissionRepository.Received(1).UserHasArtifactTypePermission(
                Arg.Is(new Guid(ObjectTypeGuids.JobHistory)), ArtifactPermission.Create);
            _sourcePermissionRepository.Received(1).UserHasArtifactInstancePermission(_validationModel.ObjectTypeGuid, _validationModel.ArtifactId, ArtifactPermission.View);
            _sourcePermissionRepository.Received(1).UserHasArtifactTypePermission(_SOURCE_PROVIDER_GUID, ArtifactPermission.View);
            _sourcePermissionRepository.Received(1).UserHasArtifactTypePermission(_DESTINATION_PROVIDER_GUID, ArtifactPermission.View);
            _sourcePermissionRepository.Received(1).UserHasArtifactInstancePermission(_SOURCE_PROVIDER_GUID, _validationModel.SourceProviderArtifactId, ArtifactPermission.View);
        }
    }
}
