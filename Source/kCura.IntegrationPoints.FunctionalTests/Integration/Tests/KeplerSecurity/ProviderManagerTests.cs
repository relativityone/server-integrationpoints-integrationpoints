using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using kCura.IntegrationPoints.Core.Provider;
using LanguageExt;
using Moq;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Services.Repositories;
using Relativity.Logging;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.KeplerSecurity
{
    class ProviderManagerTests : KeplerSecurityTestsBase
    {
        private IProviderManager _sut;

        public override void SetUp()
        {
            base.SetUp();
            
            Mock<IProviderRepository> providerRepositoryFake = new Mock<IProviderRepository>();
            providerRepositoryFake.Setup(x => x.GetSourceProviders(_WORKSPACE_ID)).Returns(new List<ProviderModel>());
            providerRepositoryFake.Setup(x => x.GetDesinationProviders(_WORKSPACE_ID)).Returns(new List<ProviderModel>());
            providerRepositoryFake.Setup(x => x.GetSourceProviderArtifactId(_WORKSPACE_ID, "")).Returns(1);
            providerRepositoryFake.Setup(x => x.GetDestinationProviderArtifactId(_WORKSPACE_ID, "")).Returns(1);
            
            Container.Register(Component.For<IProviderRepository>().UsingFactoryMethod(k => providerRepositoryFake.Object).LifestyleTransient().IsDefault());

            _sut = new ProviderManager(_loggerFake.Object, _permissionRepositoryFactoryFake.Object, Container);
        }

        [IdentifiedTestCase("D47A46E0-2BE2-41DE-9B72-9E51459FCBED", -1, false, false)]
        [IdentifiedTestCase("5D49E2BC-56E8-4D9E-9D24-9F7835B7C1B8", -1, false, true)]
        [IdentifiedTestCase("86195C10-20C5-4B37-8F25-7C30CF023D50", -1, true, false)]
        [IdentifiedTestCase("1C15B93D-552E-4F5A-B3FD-187CCE6B77E4", 1, true, true)]
        public void UserPermissionsToGetSourceProviderArtifactIdVerification(
            int expectedTotal, bool workspaceAccessPermissions, bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);
            int total = -1;

            // Act
            total = ActAndGetResult(() => _sut.GetSourceProviderArtifactIdAsync(_WORKSPACE_ID, "").Result,
                total, workspaceAccessPermissions & artifactTypePermissions);
            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions,
                UserHasCreatePermissions = artifactTypePermissions,
                UserHasEditPermissions = artifactTypePermissions,
                UserHasViewPermissions = artifactTypePermissions,
                UserHasDeletePermissions = artifactTypePermissions
            };

            // Assert
            Assert(expectedRepositoryPermissions);
        }

        [IdentifiedTestCase("BD357A65-0363-489E-BD5D-16B74926D7B2", -1, false, false)]
        [IdentifiedTestCase("6CE0B203-011B-4E93-9A6B-D36C92157E54", -1, false, true)]
        [IdentifiedTestCase("1466FCA1-98D0-4566-B386-C6A00D296C09", -1, true, false)]
        [IdentifiedTestCase("0D914254-740B-4A47-9451-967968ED33DE", 1, true, true)]
        public void UserPermissionsToGetDestinationProviderArtifactIdVerification(
            int expectedTotal, bool workspaceAccessPermissions, bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);
            int total = -1;

            // Act
            total = ActAndGetResult(() => _sut.GetDestinationProviderArtifactIdAsync(_WORKSPACE_ID, "").Result,
                total, workspaceAccessPermissions & artifactTypePermissions);

            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions,
                UserHasCreatePermissions = artifactTypePermissions,
                UserHasEditPermissions = artifactTypePermissions,
                UserHasViewPermissions = artifactTypePermissions,
                UserHasDeletePermissions = artifactTypePermissions
            };

            // Assert
            Assert(expectedRepositoryPermissions);
        }

        [IdentifiedTestCase("BD357A65-0363-489E-BD5D-16B74926D7B2", 0, false, false)]
        [IdentifiedTestCase("6CE0B203-011B-4E93-9A6B-D36C92157E54", 0, false, true)]
        [IdentifiedTestCase("1466FCA1-98D0-4566-B386-C6A00D296C09", 0, true, false)]
        [IdentifiedTestCase("0D914254-740B-4A47-9451-967968ED33DE", 0, true, true)]
        public void UserPermissionsToGetSourceProvidersVerification(
            int expectedTotal, bool workspaceAccessPermissions, bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);
            IList<ProviderModel> total = new List<ProviderModel>();

            // Act
            total = ActAndGetResult(() => _sut.GetSourceProviders(_WORKSPACE_ID).Result,
                total, workspaceAccessPermissions & artifactTypePermissions);
            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions,
                UserHasCreatePermissions = artifactTypePermissions,
                UserHasEditPermissions = artifactTypePermissions,
                UserHasViewPermissions = artifactTypePermissions,
                UserHasDeletePermissions = artifactTypePermissions
            };

            // Assert
            Assert(expectedRepositoryPermissions);
        }

        [IdentifiedTestCase("BEDE6DFD-3D03-432C-87B5-40B18EBE2A7B", 0, false, false)]
        [IdentifiedTestCase("8465B97D-F294-4FED-BAAC-405D325CE758", 0, false, true)]
        [IdentifiedTestCase("CF196867-D296-4824-B7A6-AF529BBE065D", 0, true, false)]
        [IdentifiedTestCase("1D898FED-CA03-4D5B-A40B-C11371E2A8D5", 0, true, true)]
        public void UserPermissionsToGetDestinationProvidersVerification(
            int expectedTotal, bool workspaceAccessPermissions, bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);
            IList<ProviderModel> total = new List<ProviderModel>();

            // Act
            total = ActAndGetResult(() => _sut.GetDestinationProviders(_WORKSPACE_ID).Result,
                total, workspaceAccessPermissions & artifactTypePermissions);

            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions,
                UserHasCreatePermissions = artifactTypePermissions,
                UserHasEditPermissions = artifactTypePermissions,
                UserHasViewPermissions = artifactTypePermissions,
                UserHasDeletePermissions = artifactTypePermissions
            };

            // Assert
            Assert(expectedRepositoryPermissions);
        }

        [IdentifiedTestCase("6BC44A6F-C269-47EF-B2E8-7B5D223D1539", -1, false, false)]
        [IdentifiedTestCase("6569A313-FB11-40E8-BBDB-4442881F6E27", -1, false, true)]
        [IdentifiedTestCase("AD9D2C33-493A-45CC-9D98-B8C8CFDDDA2B", -1, true, false)]
        [IdentifiedTestCase("1FF149A2-FF03-48BA-BCBB-2CF9DA95448F", 0, true, true)]
        public void UserPermissionsToInstallProviderVerification(
            int expectedTotal, bool workspaceAccessPermissions, bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);
            InstallProviderResponse installProviderResponse = new InstallProviderResponse();

            InstallProviderRequest installProviderRequest = new InstallProviderRequest
            {
                WorkspaceID = _WORKSPACE_ID,
                ProvidersToInstall = new List<InstallProviderDto>()
            };
            
            // Act
            Mock<IRipProviderInstaller> ripProviderInstallerFake = new Mock<IRipProviderInstaller>();
            ripProviderInstallerFake.Setup(x => x.InstallProvidersAsync(new List<SourceProvider>()))
                .Returns(Task.FromResult(new Either<string, Unit>()));

            Container.Register(Component.For<IRipProviderInstaller>()
                .UsingFactoryMethod(x => ripProviderInstallerFake.Object).LifestyleTransient().IsDefault());

            installProviderResponse = ActAndGetResult(() => _sut.InstallProviderAsync(installProviderRequest).Result, 
                installProviderResponse, workspaceAccessPermissions & artifactTypePermissions);
            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions,
                UserHasCreatePermissions = artifactTypePermissions,
                UserHasEditPermissions = artifactTypePermissions,
                UserHasViewPermissions = artifactTypePermissions,
                UserHasDeletePermissions = artifactTypePermissions
            };

            // Assert
            Assert(expectedRepositoryPermissions);
        }

        [IdentifiedTestCase("62F6C646-D0F0-4034-865D-F0A184352AD6", -1, false, false)]
        [IdentifiedTestCase("B1E81F86-FAF8-4650-A240-92BC1E9D91EA", -1, false, true)]
        [IdentifiedTestCase("E19736B7-EE89-4307-8EBD-11BAEA8C5C31", -1, true, false)]
        [IdentifiedTestCase("A5A201F4-A9D3-4F0A-B19C-4A3BA3773FBB", 0, true, true)]
        public void UserPermissionsToUninstallProviderVerification(
            int expectedTotal, bool workspaceAccessPermissions, bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);
            int applicationId = 432;
            UninstallProviderResponse uninstallProviderResponse = new UninstallProviderResponse();
            UninstallProviderRequest uninstallProviderRequest = new UninstallProviderRequest
            {
                WorkspaceID = _WORKSPACE_ID,
                ApplicationID = applicationId
            };

            // Act
            Mock<IRipProviderUninstaller> ripProviderInstallerFake = new Mock<IRipProviderUninstaller>();
            ripProviderInstallerFake.Setup(x => x.UninstallProvidersAsync(applicationId))
                .Returns(Task.FromResult(new Either<string, Unit>()));

            Container.Register(Component.For<IRipProviderUninstaller>()
                .UsingFactoryMethod(x => ripProviderInstallerFake.Object).LifestyleTransient().IsDefault());

            ActAndGetResult(() => _sut.UninstallProviderAsync(uninstallProviderRequest).Result, 
                uninstallProviderResponse, workspaceAccessPermissions & artifactTypePermissions);
            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions,
                UserHasCreatePermissions = artifactTypePermissions,
                UserHasEditPermissions = artifactTypePermissions,
                UserHasViewPermissions = artifactTypePermissions,
                UserHasDeletePermissions = artifactTypePermissions
            };

            // Assert
            Assert(expectedRepositoryPermissions);
        }
    }
}
