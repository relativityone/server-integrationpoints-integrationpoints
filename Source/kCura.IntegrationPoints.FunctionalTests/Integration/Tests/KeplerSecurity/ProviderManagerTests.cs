using System.Collections.Generic;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using FluentAssertions;
using kCura.IntegrationPoints.Core.Provider;
using LanguageExt;
using LanguageExt.DataTypes.Serialisation;
using Moq;
using Relativity.IntegrationPoints.Contracts;
using Relativity.IntegrationPoints.Services;
using Relativity.IntegrationPoints.Services.Repositories;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Tests.Integration.Tests.KeplerSecurity
{
    class ProviderManagerTests : KeplerSecurityTestsBase
    {
        private IProviderManager _sut;
        private Mock<IProviderRepository> _providerRepositoryFake;

        public override void SetUp()
        {
            base.SetUp();

            _providerRepositoryFake = new Mock<IProviderRepository>();
            
            Container.Register(Component.For<IProviderRepository>().UsingFactoryMethod(k => _providerRepositoryFake.Object)
                .LifestyleTransient().IsDefault());

            _sut = new ProviderManager(Logger, PermissionRepositoryFactory, Container);
        }

        [IdentifiedTestCase("D47A46E0-2BE2-41DE-9B72-9E51459FCBED", -1, false, false)]
        [IdentifiedTestCase("5D49E2BC-56E8-4D9E-9D24-9F7835B7C1B8", -1, false, true)]
        [IdentifiedTestCase("86195C10-20C5-4B37-8F25-7C30CF023D50", -1, true, false)]
        [IdentifiedTestCase("1C15B93D-552E-4F5A-B3FD-187CCE6B77E4", 123, true, true)]
        public void UserPermissionsToGetSourceProviderArtifactIdVerification(
            int expectedSourceProviderArtifactId, bool workspaceAccessPermissions, bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);
            int sourceProviderArtifactId = -1;

            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions,
                UserHasCreatePermissions = artifactTypePermissions,
                UserHasEditPermissions = artifactTypePermissions,
                UserHasViewPermissions = artifactTypePermissions,
                UserHasDeletePermissions = artifactTypePermissions
            };

            _providerRepositoryFake.Setup(x => x.GetSourceProviderArtifactId(SourceWorkspace.ArtifactId, ""))
                .Returns(expectedSourceProviderArtifactId);

            // Act
            sourceProviderArtifactId = ActAndGetResult(() => _sut.GetSourceProviderArtifactIdAsync(SourceWorkspace.ArtifactId, "").Result,
                sourceProviderArtifactId, workspaceAccessPermissions & artifactTypePermissions);
            

            // Assert
            Assert(expectedRepositoryPermissions);
            sourceProviderArtifactId.ShouldBeEquivalentTo(expectedSourceProviderArtifactId);
        }

        [IdentifiedTestCase("BD357A65-0363-489E-BD5D-16B74926D7B2", -1, false, false)]
        [IdentifiedTestCase("6CE0B203-011B-4E93-9A6B-D36C92157E54", -1, false, true)]
        [IdentifiedTestCase("1466FCA1-98D0-4566-B386-C6A00D296C09", -1, true, false)]
        [IdentifiedTestCase("0D914254-740B-4A47-9451-967968ED33DE", 123, true, true)]
        public void UserPermissionsToGetDestinationProviderArtifactIdVerification(
            int expectedDestinationProviderArtifactId, bool workspaceAccessPermissions, bool artifactTypePermissions)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);
            int destinationProviderArtifactId = -1;

            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions,
                UserHasCreatePermissions = artifactTypePermissions,
                UserHasEditPermissions = artifactTypePermissions,
                UserHasViewPermissions = artifactTypePermissions,
                UserHasDeletePermissions = artifactTypePermissions
            };

            _providerRepositoryFake.Setup(x => x.GetDestinationProviderArtifactId(SourceWorkspace.ArtifactId, ""))
                .Returns(expectedDestinationProviderArtifactId);

            // Act
            destinationProviderArtifactId = ActAndGetResult(() => _sut.GetDestinationProviderArtifactIdAsync(SourceWorkspace.ArtifactId, "").Result,
                destinationProviderArtifactId, workspaceAccessPermissions & artifactTypePermissions);

            // Assert
            Assert(expectedRepositoryPermissions);
            destinationProviderArtifactId.ShouldBeEquivalentTo(expectedDestinationProviderArtifactId);
        }

        [IdentifiedTestCase("BD357A65-0363-489E-BD5D-16B74926D7B2", false, false, 0, null)]
        [IdentifiedTestCase("6CE0B203-011B-4E93-9A6B-D36C92157E54", false, true, 0, null)]
        [IdentifiedTestCase("1466FCA1-98D0-4566-B386-C6A00D296C09", true, false, 0, null)]
        [IdentifiedTestCase("0D914254-740B-4A47-9451-967968ED33DE", true, true, 10, "example name")]
        public void UserPermissionsToGetSourceProvidersVerification(bool workspaceAccessPermissions, bool artifactTypePermissions,
            int expectedArtifactId, string expectedName)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);
            
            IList<ProviderModel> providerModels = new List<ProviderModel>
            {
                new ProviderModel
                {
                    ArtifactId = 0,
                    Name = null
                }
            };

            IList<ProviderModel> expectedProviderModels = new List<ProviderModel>
            {
                new ProviderModel
                {
                    ArtifactId = expectedArtifactId,
                    Name = expectedName
                }
            };

            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions,
                UserHasCreatePermissions = artifactTypePermissions,
                UserHasEditPermissions = artifactTypePermissions,
                UserHasViewPermissions = artifactTypePermissions,
                UserHasDeletePermissions = artifactTypePermissions
            };

            _providerRepositoryFake.Setup(x => x.GetSourceProviders(SourceWorkspace.ArtifactId))
                .Returns(expectedProviderModels);
            // Act
            providerModels = ActAndGetResult(() => _sut.GetSourceProviders(SourceWorkspace.ArtifactId).Result,
                providerModels, workspaceAccessPermissions & artifactTypePermissions);

            // Assert
            Assert(expectedRepositoryPermissions);
            providerModels[0].ArtifactId.ShouldBeEquivalentTo(expectedArtifactId);
            providerModels[0].Name.ShouldBeEquivalentTo(expectedName);
        }

        [IdentifiedTestCase("BEDE6DFD-3D03-432C-87B5-40B18EBE2A7B", false, false, 0, null)]
        [IdentifiedTestCase("8465B97D-F294-4FED-BAAC-405D325CE758", false, true, 0, null)]
        [IdentifiedTestCase("CF196867-D296-4824-B7A6-AF529BBE065D", true, false, 0, null)]
        [IdentifiedTestCase("1D898FED-CA03-4D5B-A40B-C11371E2A8D5", true, true, 123, "example name")]
        public void UserPermissionsToGetDestinationProvidersVerification(bool workspaceAccessPermissions, bool artifactTypePermissions,
            int expectedArtifactId, string expectedName)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);
            IList<ProviderModel> providerModels = new List<ProviderModel>
            {
                new ProviderModel
                {
                    ArtifactId = 0,
                    Name = null
                }
            };

            IList<ProviderModel> expectedProviderModels = new List<ProviderModel>
            {
                new ProviderModel
                {
                    ArtifactId = expectedArtifactId,
                    Name = expectedName
                }
            };

            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions,
                UserHasCreatePermissions = artifactTypePermissions,
                UserHasEditPermissions = artifactTypePermissions,
                UserHasViewPermissions = artifactTypePermissions,
                UserHasDeletePermissions = artifactTypePermissions
            };

            _providerRepositoryFake.Setup(x => x.GetDesinationProviders(SourceWorkspace.ArtifactId))
                .Returns(expectedProviderModels);

            // Act
            providerModels = ActAndGetResult(() => _sut.GetDestinationProviders(SourceWorkspace.ArtifactId).Result,
                providerModels, workspaceAccessPermissions & artifactTypePermissions);

            // Assert
            Assert(expectedRepositoryPermissions);
            providerModels[0].ArtifactId.ShouldBeEquivalentTo(expectedArtifactId);
            providerModels[0].Name.ShouldBeEquivalentTo(expectedName);
        }

        [IdentifiedTestCase("6BC44A6F-C269-47EF-B2E8-7B5D223D1539", false, false, null, true)]
        [IdentifiedTestCase("6569A313-FB11-40E8-BBDB-4442881F6E27", false, true, null, true)]
        [IdentifiedTestCase("AD9D2C33-493A-45CC-9D98-B8C8CFDDDA2B", true, false, null, true)]
        [IdentifiedTestCase("1FF149A2-FF03-48BA-BCBB-2CF9DA95448F",  true, true, "", false)]
        public void UserPermissionsToInstallProviderVerification(bool workspaceAccessPermissions, bool artifactTypePermissions,
            string expectedErrorMessage, bool expectedSuccess)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);
            InstallProviderRequest installProviderRequest = new InstallProviderRequest
            {
                WorkspaceID = SourceWorkspace.ArtifactId,
                ProvidersToInstall = new List<InstallProviderDto>()
            };
            
            InstallProviderResponse installProviderResponse = new InstallProviderResponse();

            Either<string, Unit> expectedResult = new Either<string, Unit>(new List<EitherData<string, Unit>>
            {
                new EitherData<string, Unit>(EitherStatus.IsLeft, Unit.Default, "")
            });
            
            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions,
                UserHasCreatePermissions = artifactTypePermissions,
                UserHasEditPermissions = artifactTypePermissions,
                UserHasViewPermissions = artifactTypePermissions,
                UserHasDeletePermissions = artifactTypePermissions
            };

            Mock<IRipProviderInstaller> ripProviderInstallerFake = new Mock<IRipProviderInstaller>();
            ripProviderInstallerFake.Setup(x => x.InstallProvidersAsync(new List<SourceProvider>()))
                .Returns(Task.FromResult(expectedResult));

            Container.Register(Component.For<IRipProviderInstaller>()
                .UsingFactoryMethod(x => ripProviderInstallerFake.Object).LifestyleTransient().IsDefault());

            // Act
            installProviderResponse = ActAndGetResult(() => _sut.InstallProviderAsync(installProviderRequest).Result, 
                installProviderResponse, workspaceAccessPermissions & artifactTypePermissions);

            // Assert
            Assert(expectedRepositoryPermissions);
            installProviderResponse.ErrorMessage.ShouldBeEquivalentTo(expectedErrorMessage);
            installProviderResponse.Success.ShouldBeEquivalentTo(expectedSuccess);
        }

        [IdentifiedTestCase("62F6C646-D0F0-4034-865D-F0A184352AD6", false, false, null, true)]
        [IdentifiedTestCase("B1E81F86-FAF8-4650-A240-92BC1E9D91EA", false, true, null, true)]
        [IdentifiedTestCase("E19736B7-EE89-4307-8EBD-11BAEA8C5C31", true, false, null, true)]
        [IdentifiedTestCase("A5A201F4-A9D3-4F0A-B19C-4A3BA3773FBB", true, true, "", false)]
        public void UserPermissionsToUninstallProviderVerification(bool workspaceAccessPermissions, bool artifactTypePermissions,
            string expectedErrorMessage, bool expectedSuccess)
        {
            // Arrange
            Arrange(workspaceAccessPermissions, artifactTypePermissions);
            int applicationId = 432;
            UninstallProviderResponse uninstallProviderResponse = new UninstallProviderResponse();
            
            UninstallProviderRequest uninstallProviderRequest = new UninstallProviderRequest
            {
                WorkspaceID = SourceWorkspace.ArtifactId,
                ApplicationID = applicationId
            };

            Either<string, Unit> expectedResult = new Either<string, Unit>(new List<EitherData<string, Unit>>
            {
                new EitherData<string, Unit>(EitherStatus.IsLeft, Unit.Default, "")
            });

            RepositoryPermissions expectedRepositoryPermissions = new RepositoryPermissions
            {
                UserHasWorkspaceAccessPermissions = workspaceAccessPermissions,
                UserHasCreatePermissions = artifactTypePermissions,
                UserHasEditPermissions = artifactTypePermissions,
                UserHasViewPermissions = artifactTypePermissions,
                UserHasDeletePermissions = artifactTypePermissions
            };

            Mock<IRipProviderUninstaller> ripProviderUninstallerFake = new Mock<IRipProviderUninstaller>();
            ripProviderUninstallerFake.Setup(x => x.UninstallProvidersAsync(applicationId))
                .Returns(Task.FromResult(expectedResult));

            Container.Register(Component.For<IRipProviderUninstaller>()
                .UsingFactoryMethod(x => ripProviderUninstallerFake.Object).LifestyleTransient().IsDefault());

            // Act
            uninstallProviderResponse = ActAndGetResult(() => _sut.UninstallProviderAsync(uninstallProviderRequest).Result, 
                uninstallProviderResponse, workspaceAccessPermissions & artifactTypePermissions);
            
            // Assert
            Assert(expectedRepositoryPermissions);
            uninstallProviderResponse.ErrorMessage.ShouldBeEquivalentTo(expectedErrorMessage);
            uninstallProviderResponse.Success.ShouldBeEquivalentTo(expectedSuccess);
        }
    }
}
