using System.Collections.Generic;
using NUnit.Framework;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Tests.Integration.Utils;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.KeplerSecurity
{
    public class DocumentManagerTests : KeplerSecurityTestBase
    {
        private IDocumentManager _sut;

        [SetUp]
        public void Setup() => _sut = new DocumentManager(Logger, PermissionRepositoryFactory, Container);

        [Test]
        public void GetPercentagePushedToReviewAsync_ShouldNotThrow_WhenAllPermissionsAreGranted()
        {
            ShouldPassWithAllPermissions<PermissionsForDocumentManager>(() =>
            {
                PercentagePushedToReviewRequest percentagePushedToReviewRequest = new PercentagePushedToReviewRequest
                {
                    WorkspaceArtifactId = WorkspaceId
                };

                // Act
                return _sut.GetPercentagePushedToReviewAsync(percentagePushedToReviewRequest);
            });
        }

        [TestCaseSource(typeof(PermissionsForDocumentManager))]
        public void CreateIntegrationPointFromProfileAsync_ShouldThrowInsufficientPermissions(PermissionSetup[] permissionSetups)
        {
            ShouldThrowInsufficientPermissions(permissionSetups,() =>
            {
                PercentagePushedToReviewRequest percentagePushedToReviewRequest = new PercentagePushedToReviewRequest
                {
                    WorkspaceArtifactId = WorkspaceId
                };

                // Act
                return _sut.GetPercentagePushedToReviewAsync(percentagePushedToReviewRequest);
            });
        }

        [Test]
        public void GetCurrentPromotionStatusAsync_ShouldNotThrow_WhenAllPermissionsAreGranted()
        {
            ShouldPassWithAllPermissions<PermissionsForDocumentManager>(() =>
            {
                CurrentPromotionStatusRequest currentPromotionStatusRequest = new CurrentPromotionStatusRequest
                {
                    WorkspaceArtifactId = WorkspaceId
                };

                // Act
                return _sut.GetCurrentPromotionStatusAsync(currentPromotionStatusRequest);
            });
        }

        [TestCaseSource(typeof(PermissionsForDocumentManager))]
        public void GetCurrentPromotionStatusAsync_ShouldThrowInsufficientPermissions(PermissionSetup[] permissionSetups)
        {
            ShouldThrowInsufficientPermissions(permissionSetups,() =>
            {
                CurrentPromotionStatusRequest currentPromotionStatusRequest = new CurrentPromotionStatusRequest
                {
                    WorkspaceArtifactId = WorkspaceId
                };

                // Act
                return _sut.GetCurrentPromotionStatusAsync(currentPromotionStatusRequest);
            });
        }

        [Test]
        public void GetHistoricalPromotionStatusAsync_ShouldNotThrow_WhenAllPermissionsAreGranted()
        {
            ShouldPassWithAllPermissions<PermissionsForDocumentManager>(() =>
            {
                CurrentPromotionStatusRequest currentPromotionStatusRequest = new CurrentPromotionStatusRequest
                {
                    WorkspaceArtifactId = WorkspaceId
                };

                // Act
                return _sut.GetCurrentPromotionStatusAsync(currentPromotionStatusRequest);
            });
        }

        [TestCaseSource(typeof(PermissionsForDocumentManager))]
        public void GetHistoricalPromotionStatusAsync_ShouldThrowInsufficientPermissions(PermissionSetup[] permissionSetups)
        {
            ShouldThrowInsufficientPermissions(permissionSetups,() =>
            {
                HistoricalPromotionStatusRequest createIntegrationPointRequest = new HistoricalPromotionStatusRequest
                {
                    WorkspaceArtifactId = WorkspaceId
                };

                // Act
                return _sut.GetHistoricalPromotionStatusAsync(createIntegrationPointRequest);
            });
        }

      #region Permissions

        class PermissionsForDocumentManager : PermissionPermutator
        {
            protected override IEnumerable<PermissionSetup> NeededPermissions => new[]
            {
                GetPermissionRefForWorkspace(WorkspaceId)
            };
        }

        #endregion
    }
}
