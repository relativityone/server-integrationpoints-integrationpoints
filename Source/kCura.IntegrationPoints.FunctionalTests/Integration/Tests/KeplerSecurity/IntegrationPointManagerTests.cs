using System.Collections.Generic;
using Castle.MicroKernel.Registration;
using Moq;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Services.Repositories;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.KeplerSecurity
{
    class IntegrationPointManagerTests : KeplerSecurityTestsBase
    {
        private IIntegrationPointManager _sut;
        private Mock<IIntegrationPointRepository> _integrationPointProfileRepositoryFake;
        private int _INTEGRATION_POINT_ARTIFACT_ID = 554556;

        public override void SetUp()
        {
            base.SetUp();

           _integrationPointProfileRepositoryFake =
                new Mock<IIntegrationPointRepository>();

            Container.Register(Component.For<IIntegrationPointRepository>()
                .UsingFactoryMethod(_ => _integrationPointProfileRepositoryFake.Object).LifestyleTransient()
                .IsDefault());

            _sut = new IntegrationPointManager(_loggerFake.Object, _permissionRepositoryFactoryFake.Object,
                Container);
        }

        [IdentifiedTestCase("EB9CFEBC-C65C-418C-AD64-28E2E8076A40", false, false)]
        [IdentifiedTestCase("E0572DA7-4C55-486F-984E-8CD05995B8D7", false, true)]
        [IdentifiedTestCase("CD265661-463E-4E49-B74C-9065E0C1D894", true, false)]
        [IdentifiedTestCase("CD403829-62C5-4F3D-961C-324D74B14C25", true, true)]
        public void UserPermissionsToCreateIntegrationPointVerification(bool workspaceAccessPermissions,
            bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);
            CreateIntegrationPointRequest createIntegrationPointRequest = new CreateIntegrationPointRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            };

            _integrationPointProfileRepositoryFake
                .Setup(x => x.CreateIntegrationPoint(createIntegrationPointRequest))
                .Returns(new IntegrationPointModel());

            IntegrationPointModel integrationPointModel = new IntegrationPointModel();

            // Act
            integrationPointModel = ActAndGetResult(
                () => _sut.CreateIntegrationPointAsync(createIntegrationPointRequest).Result,
                integrationPointModel);

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

        [IdentifiedTestCase("3CA2578E-BDC3-4D55-85B5-D9623684E601", false, false)]
        [IdentifiedTestCase("342D43EB-EC37-490B-876C-29677914D6BD", false, true)]
        [IdentifiedTestCase("3DD7AFA2-4005-4F2A-8DA1-73FB32E7F95B", true, false)]
        [IdentifiedTestCase("F05958B0-0260-407F-A938-49F637EB8069", true, true)]
        public void UserPermissionsToUpdateIntegrationPointVerification(bool workspaceAccessPermissions,
            bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);
            UpdateIntegrationPointRequest updateIntegrationPointRequest = new UpdateIntegrationPointRequest
            {
                WorkspaceArtifactId = _WORKSPACE_ID
            };

            _integrationPointProfileRepositoryFake
                .Setup(x => x.UpdateIntegrationPoint(updateIntegrationPointRequest))
                .Returns(new IntegrationPointModel());

            IntegrationPointModel integrationPointModel = new IntegrationPointModel();

            // Act
            integrationPointModel = ActAndGetResult(
                () => _sut.UpdateIntegrationPointAsync(updateIntegrationPointRequest).Result,
                integrationPointModel);

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

        [IdentifiedTestCase("A0EE0D92-0A22-4BB5-B536-C09E2BF3943F", false, false)]
        [IdentifiedTestCase("07177424-90DD-420E-8F75-D635822479AA", false, true)]
        [IdentifiedTestCase("E83B8598-10CC-42DC-930B-1D8C068090AB", true, false)]
        [IdentifiedTestCase("B677CB2F-803A-4BBF-9962-6801B5C8FF9C", true, true)]
        public void UserPermissionsToGetIntegrationPointVerification(bool workspaceAccessPermissions,
            bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);

            _integrationPointProfileRepositoryFake
                .Setup(x => x.GetIntegrationPoint(_INTEGRATION_POINT_ARTIFACT_ID))
                .Returns(new IntegrationPointModel());

            IntegrationPointModel integrationPointModel = new IntegrationPointModel();

            // Act
            integrationPointModel = ActAndGetResult(
                () => _sut.GetIntegrationPointAsync(_WORKSPACE_ID, _INTEGRATION_POINT_ARTIFACT_ID).Result,
                integrationPointModel);

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

        [IdentifiedTestCase("1E8A97B0-C0BC-4F05-9479-92D3375B7FC8", false, false)]
        [IdentifiedTestCase("A14CB4EB-0D4B-44ED-BB61-2A245FA3D0FB", false, true)]
        [IdentifiedTestCase("9C699DD7-E4FD-48BC-A950-CF1CBC96C3D3", true, false)]
        [IdentifiedTestCase("49278DE6-F4F7-4306-8E3D-05676A61DA69", true, true)]
        public void UserPermissionsToRunIntegrationPointVerification(bool workspaceAccessPermissions,
            bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);

            _integrationPointProfileRepositoryFake
                .Setup(x => x.RunIntegrationPoint(_WORKSPACE_ID, _INTEGRATION_POINT_ARTIFACT_ID))
                .Returns(new IntegrationPointModel());

            object objectInstance = new object();

            // Act
            objectInstance = ActAndGetResult(
                () => _sut.GetIntegrationPointAsync(_WORKSPACE_ID, _INTEGRATION_POINT_ARTIFACT_ID).Result,
                objectInstance);

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

        [IdentifiedTestCase("DEBA0180-AAA8-441F-BA7A-3BE80F28455E", false, false)]
        [IdentifiedTestCase("AA031A96-54FF-4B52-B3FA-52F623B51367", false, true)]
        [IdentifiedTestCase("04423040-B2D0-4F0A-B998-DF84331AB519", true, false)]
        [IdentifiedTestCase("FC343DB2-0C31-40AF-9AEA-A9EDBFD1FAE8", true, true)]
        public void UserPermissionsToGetAllIntegrationPointsVerification(bool workspaceAccessPermissions,
            bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);

            _integrationPointProfileRepositoryFake
                .Setup(x => x.GetAllIntegrationPoints())
                .Returns(new List<IntegrationPointModel>());

            IList<IntegrationPointModel> integrationPointModels = new List<IntegrationPointModel>();

            // Act
            integrationPointModels = ActAndGetResult(
                () => _sut.GetAllIntegrationPointsAsync(_WORKSPACE_ID).Result,
                integrationPointModels);

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

        [IdentifiedTestCase("0D065A1B-4F8A-4962-8AA1-F64A30706335", false, false)]
        [IdentifiedTestCase("4EC62AE9-A849-4DA7-91D4-364B00B4514E", false, true)]
        [IdentifiedTestCase("71120E2F-6C57-4EDC-BD7F-C3F25735DA04", true, false)]
        [IdentifiedTestCase("C6F4D8B2-A685-447A-9438-AC7D797D6F79", true, true)]
        public void UserPermissionsToGetOverwriteFieldsChoicesVerification(bool workspaceAccessPermissions,
            bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);

            _integrationPointProfileRepositoryFake
                .Setup(x => x.GetOverwriteFieldChoices())
                .Returns(new List<OverwriteFieldsModel>());

            IList<OverwriteFieldsModel> overwriteFieldsModels = new List<OverwriteFieldsModel>();

            // Act
            overwriteFieldsModels = ActAndGetResult(
                () => _sut.GetOverwriteFieldsChoicesAsync(_WORKSPACE_ID).Result,
                overwriteFieldsModels);

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

        [IdentifiedTestCase("2AB2964E-EE15-4303-A080-63D7A0552109", false, false)]
        [IdentifiedTestCase("6632F27B-FE81-4557-AD87-EDE130E8FBF6", false, true)]
        [IdentifiedTestCase("17415F7D-D565-483C-A9C5-F6BB01092B2A", true, false)]
        [IdentifiedTestCase("DEBCA9EC-4E56-452C-83E1-E4B1063AA84B", true, true)]
        public void UserPermissionsToCreateIntegrationPointFromProfileVerification(bool workspaceAccessPermissions,
            bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);

            _integrationPointProfileRepositoryFake
                .Setup(x => x.GetOverwriteFieldChoices())
                .Returns(new List<OverwriteFieldsModel>());

            IntegrationPointModel integrationPointModel = new IntegrationPointModel();

            // Act
            integrationPointModel = ActAndGetResult(
                () => _sut.CreateIntegrationPointFromProfileAsync(_WORKSPACE_ID, 32142, "example name").Result,
                integrationPointModel);

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

        [IdentifiedTestCase("968D972F-5562-4A08-9B62-CD2CDEA8AC96", false, false)]
        [IdentifiedTestCase("94051369-EB33-4A54-AD80-2C99765A80AA", false, true)]
        [IdentifiedTestCase("3E7481B0-8E71-4042-88FE-1990B3FB9722", true, false)]
        [IdentifiedTestCase("7E733FDF-6502-4B76-A317-E1C07DD164F2", true, true)]
        public void UserPermissionsToGetIntegrationPointArtifactTypeIdVerification(bool workspaceAccessPermissions,
            bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);

            _integrationPointProfileRepositoryFake
                .Setup(x => x.GetIntegrationPointArtifactTypeId())
                .Returns(-1);

            int artifactTypeId = -1;

            // Act
            artifactTypeId = ActAndGetResult(
                () => _sut.GetIntegrationPointArtifactTypeIdAsync(_WORKSPACE_ID).Result,
                artifactTypeId);

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
