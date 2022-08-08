using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Models;
using kCura.IntegrationPoints.Core.Validation.Parts;
using kCura.IntegrationPoints.Data;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Core.Tests.Validation
{
    [TestFixture, Category("Unit")]
    public class ViewErrorsPermissionValidatorTests : PermissionValidatorTestsBase
    {
        [TestCase(true, true)]
        [TestCase(true, false)]
        [TestCase(false, true)]
        [TestCase(false, false)]
        public void ValidateTest(bool hasJobHistoryViewPermission, bool hasJobHistoryErrorViewPermission)
        {
            // arrange
            var workspaceArtifactId = _SOURCE_WORKSPACE_ID;

            _sourcePermissionRepository.UserHasArtifactTypePermission(Arg.Is<Guid>(guid => guid == new Guid(ObjectTypeGuids.JobHistory)), ArtifactPermission.View).Returns(hasJobHistoryViewPermission);
            _sourcePermissionRepository.UserHasArtifactTypePermission(Arg.Is<Guid>(guid => guid == new Guid(ObjectTypeGuids.JobHistoryError)), ArtifactPermission.View).Returns(hasJobHistoryErrorViewPermission);

            var stopJobPermissionValidator = new ViewErrorsPermissionValidator(_repositoryFactory);

            // act
            var validationResult = stopJobPermissionValidator.Validate(workspaceArtifactId);

            // assert
            validationResult.Check(hasJobHistoryViewPermission && hasJobHistoryErrorViewPermission);

            _sourcePermissionRepository.Received(1).UserHasArtifactTypePermission(Arg.Is<Guid>(guid => guid == new Guid(ObjectTypeGuids.JobHistory)), ArtifactPermission.View);
            _sourcePermissionRepository.Received(1).UserHasArtifactTypePermission(Arg.Is<Guid>(guid => guid == new Guid(ObjectTypeGuids.JobHistoryError)), ArtifactPermission.View);
        }
    }
}
