using System;
using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Moq;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Services.Repositories;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.KeplerSecurity
{
    class JobHistoryManagerTests : KeplerSecurityTestsBase
    {
        private IJobHistoryManager _sut;

        public override void SetUp()
        {
            base.SetUp();
            
            _sut = new JobHistoryManager(_loggerFake.Object, _permissionRepositoryFactoryFake.Object, Container);
        }

        [IdentifiedTestCase("A41778E7-2D25-49FA-9919-ECD90B2168BD", false, false)]
        [IdentifiedTestCase("DFAED5F1-2DC8-4CAA-8BDD-C99542DE537D", false, true)]
        [IdentifiedTestCase("55190915-CEE6-4AB9-99A0-5B2586E5A84B", true, false)]
        [IdentifiedTestCase("8E0588E1-60E0-4876-A220-04C8557565D7", true, true)]
        public void UserPermissionsToGetJobHistoryVerification( bool workspaceAccessPermissions, bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);

            JobHistoryRequest jobHistoryRequest = new JobHistoryRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            };
            JobHistorySummaryModel jobHistorySummaryModel = new JobHistorySummaryModel();

                Mock<IJobHistoryRepository> jobHistoryRepositoryFake = new Mock<IJobHistoryRepository>();
            jobHistoryRepositoryFake.Setup(x => x.GetJobHistory(jobHistoryRequest)).Returns(new JobHistorySummaryModel());

            Container.Register(Component.For<IJobHistoryRepository>()
                .UsingFactoryMethod(_ => jobHistoryRepositoryFake.Object).LifestyleTransient().IsDefault());

            // Act
            jobHistorySummaryModel = ActAndGetResult(() => _sut.GetJobHistoryAsync(jobHistoryRequest).Result,
                jobHistorySummaryModel);

            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions,
                UserHasCreatePermissions = artifactTypePermissions,
                UserHasEditPermissions = artifactTypePermissions,
                UserHasViewPermissions = artifactTypePermissions,
                UserHasDeletePermissions = artifactTypePermissions
            };

            // Assert
            Assert(-1, -1, expectedRepositoryPermissions);
        }

    }
}
