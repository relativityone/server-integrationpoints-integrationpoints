using Castle.Windsor;
using FluentAssertions;
using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Core.Provider;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using LanguageExt;
using Moq;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using NUnit.Framework;
using Relativity.API;
using Relativity.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Relativity.IntegrationPoints.Services.Helpers;
using Relativity.IntegrationPoints.Services.Repositories;
using static LanguageExt.Prelude;
using SourceProvider = Relativity.IntegrationPoints.Contracts.SourceProvider;

namespace Relativity.IntegrationPoints.Services.Tests.Managers
{
    [TestFixture, Category("Unit")]
    public class ProviderManagerTests : TestBase
    {
        private const int _WORKSPACE_ID = 266818;
        private ProviderManager _providerManager;
        private IPermissionRepository _permissionRepository;
        private ILog _logger;
        private IWindsorContainer _container;

        public override void SetUp()
        {
            _logger = Substitute.For<ILog>();
            _permissionRepository = Substitute.For<IPermissionRepository>();
            _container = Substitute.For<IWindsorContainer>();

            var permissionRepositoryFactory = Substitute.For<IPermissionRepositoryFactory>();
            permissionRepositoryFactory.Create(Arg.Any<IHelper>(), _WORKSPACE_ID).Returns(_permissionRepository);

            _providerManager = new ProviderManager(_logger, permissionRepositoryFactory, _container);
        }

        [Test]
        public void ItShouldGrantAccessForSourceProvider()
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.SourceProvider), ArtifactPermission.View).Returns(true);

            _providerManager.GetSourceProviderArtifactIdAsync(_WORKSPACE_ID, Guid.NewGuid().ToString()).Wait();
            _providerManager.GetSourceProviders(_WORKSPACE_ID).Wait();

            _permissionRepository.Received(2).UserHasPermissionToAccessWorkspace();
            _permissionRepository.Received(2).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.SourceProvider), ArtifactPermission.View);
        }

        [Test]
        public void ItShouldGrantAccessForDestinationProvider()
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.DestinationProvider), ArtifactPermission.View).Returns(true);

            _providerManager.GetDestinationProviderArtifactIdAsync(_WORKSPACE_ID, Guid.NewGuid().ToString()).Wait();
            _providerManager.GetDestinationProviders(_WORKSPACE_ID).Wait();

            _permissionRepository.Received(2).UserHasPermissionToAccessWorkspace();
            _permissionRepository.Received(2).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.DestinationProvider), ArtifactPermission.View);
        }

        [Test]
        [TestCase(false, false, "Workspace, Source Provider - View")]
        [TestCase(false, true, "Workspace")]
        [TestCase(true, false, "Source Provider - View")]
        public void ItShouldDenyAccessForSourceProviderAndLogIt(bool workspaceAccess, bool sourceProviderAccess, string missingPermissions)
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(workspaceAccess);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.SourceProvider), ArtifactPermission.View).Returns(sourceProviderAccess);

            Assert.That(() => _providerManager.GetSourceProviderArtifactIdAsync(_WORKSPACE_ID, Guid.NewGuid().ToString()).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
                    .And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

            Assert.That(() => _providerManager.GetSourceProviders(_WORKSPACE_ID).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
                    .And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

            _permissionRepository.Received(2).UserHasPermissionToAccessWorkspace();
            _permissionRepository.Received(2).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.SourceProvider), ArtifactPermission.View);

            _logger.Received(1)
                .LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", "GetSourceProviderArtifactIdAsync",
                    missingPermissions);
            _logger.Received(1)
                .LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", "GetSourceProviders", missingPermissions);
        }

        [Test]
        [TestCase(false, false, "Workspace, Destination Provider - View")]
        [TestCase(false, true, "Workspace")]
        [TestCase(true, false, "Destination Provider - View")]
        public void ItShouldDenyAccessForDestinationProviderAndLogIt(bool workspaceAccess, bool destinationProviderAccess, string missingPermissions)
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(workspaceAccess);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.DestinationProvider), ArtifactPermission.View).Returns(destinationProviderAccess);

            Assert.That(() => _providerManager.GetDestinationProviderArtifactIdAsync(_WORKSPACE_ID, Guid.NewGuid().ToString()).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
                    .And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

            Assert.That(() => _providerManager.GetDestinationProviders(_WORKSPACE_ID).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InsufficientPermissionException>()
                    .And.With.InnerException.Message.EqualTo("You do not have permission to access this service."));

            _permissionRepository.Received(2).UserHasPermissionToAccessWorkspace();
            _permissionRepository.Received(2).UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.DestinationProvider), ArtifactPermission.View);

            _logger.Received(1)
                .LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", "GetDestinationProviderArtifactIdAsync",
                    missingPermissions);
            _logger.Received(1)
                .LogError("User doesn't have permission to access endpoint {endpointName}. Missing permissions {missingPermissions}.", "GetDestinationProviders", missingPermissions);
        }

        [Test]
        public void ItShouldHideAndLogException()
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.SourceProvider), ArtifactPermission.View).Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.DestinationProvider), ArtifactPermission.View).Returns(true);

            var expectedException = new ArgumentException();

            var providerRepository = Substitute.For<IProviderAccessor>();
            providerRepository.GetDesinationProviders(Arg.Any<int>()).Throws(expectedException);
            providerRepository.GetDestinationProviderArtifactId(Arg.Any<int>(), Arg.Any<string>()).Throws(expectedException);
            providerRepository.GetSourceProviders(Arg.Any<int>()).Throws(expectedException);
            providerRepository.GetSourceProviderArtifactId(Arg.Any<int>(), Arg.Any<string>()).Throws(expectedException);

            _container.Resolve<IProviderAccessor>().Returns(providerRepository);

            Assert.That(() => _providerManager.GetDestinationProviderArtifactIdAsync(_WORKSPACE_ID, Guid.NewGuid().ToString()).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
                    .And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));

            Assert.That(() => _providerManager.GetDestinationProviders(_WORKSPACE_ID).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
                    .And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));

            Assert.That(() => _providerManager.GetSourceProviderArtifactIdAsync(_WORKSPACE_ID, Guid.NewGuid().ToString()).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
                    .And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));

            Assert.That(() => _providerManager.GetSourceProviders(_WORKSPACE_ID).Wait(),
                Throws.Exception.With.InnerException.TypeOf<InternalServerErrorException>()
                    .And.With.InnerException.Message.EqualTo("Error occurred during request processing. Please contact your administrator."));

            _logger.Received(1)
                .LogError(expectedException, "Error occurred during request processing in {endpointName}.", "GetDestinationProviderArtifactIdAsync");
            _logger.Received(1)
                .LogError(expectedException, "Error occurred during request processing in {endpointName}.", "GetDestinationProviders");
            _logger.Received(1)
                .LogError(expectedException, "Error occurred during request processing in {endpointName}.", "GetSourceProviderArtifactIdAsync");
            _logger.Received(1)
                .LogError(expectedException, "Error occurred during request processing in {endpointName}.", "GetSourceProviders");
        }

        [Test]
        public void ItShouldReturnAllSourceProviders()
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.SourceProvider), ArtifactPermission.View).Returns(true);

            var providerRepository = Substitute.For<IProviderAccessor>();
            _container.Resolve<IProviderAccessor>().Returns(providerRepository);

            var expectedResult = new List<ProviderModel>();
            providerRepository.GetSourceProviders(_WORKSPACE_ID).Returns(expectedResult);

            var actualResult = _providerManager.GetSourceProviders(_WORKSPACE_ID).Result;

            providerRepository.Received(1).GetSourceProviders(_WORKSPACE_ID);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ItShouldReturnSourceProvider()
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.SourceProvider), ArtifactPermission.View).Returns(true);

            var providerRepository = Substitute.For<IProviderAccessor>();
            _container.Resolve<IProviderAccessor>().Returns(providerRepository);

            var guid = Guid.NewGuid();

            var expectedResult = 892946;
            providerRepository.GetSourceProviderArtifactId(_WORKSPACE_ID, guid.ToString()).Returns(expectedResult);

            var actualResult = _providerManager.GetSourceProviderArtifactIdAsync(_WORKSPACE_ID, guid.ToString()).Result;

            providerRepository.Received(1).GetSourceProviderArtifactId(_WORKSPACE_ID, guid.ToString());

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ItShouldReturnAllDestinationProviders()
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.DestinationProvider), ArtifactPermission.View).Returns(true);

            var providerRepository = Substitute.For<IProviderAccessor>();
            _container.Resolve<IProviderAccessor>().Returns(providerRepository);

            var expectedResult = new List<ProviderModel>();
            providerRepository.GetDesinationProviders(_WORKSPACE_ID).Returns(expectedResult);

            var actualResult = _providerManager.GetDestinationProviders(_WORKSPACE_ID).Result;

            providerRepository.Received(1).GetDesinationProviders(_WORKSPACE_ID);

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void ItShouldReturnDestinationProvider()
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);
            _permissionRepository.UserHasArtifactTypePermission(new Guid(ObjectTypeGuids.DestinationProvider), ArtifactPermission.View).Returns(true);

            var providerRepository = Substitute.For<IProviderAccessor>();
            _container.Resolve<IProviderAccessor>().Returns(providerRepository);

            var guid = Guid.NewGuid();

            var expectedResult = 118867;
            providerRepository.GetDestinationProviderArtifactId(_WORKSPACE_ID, guid.ToString()).Returns(expectedResult);

            var actualResult = _providerManager.GetDestinationProviderArtifactIdAsync(_WORKSPACE_ID, guid.ToString()).Result;

            providerRepository.Received(1).GetDestinationProviderArtifactId(_WORKSPACE_ID, guid.ToString());

            Assert.That(actualResult, Is.EqualTo(expectedResult));
        }

        [Test]
        public void InstallProviderAsyncShouldDenyAccessWhenMissingWorkspacePermission()
        {
            // arrange
            AddAllRequiredPermissionForInstallProviderAsync();
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(false);

            SetupRipProviderInstallerSuccessfullResponse();

            InstallProviderRequest request = GetInstallProviderRequest();

            // act
            Func<Task> intallProviderAction = async () => await _providerManager.InstallProviderAsync(request).ConfigureAwait(false);

            // assert
            intallProviderAction.ShouldThrow<InsufficientPermissionException>();
        }

        [TestCase(ArtifactPermission.Create)]
        [TestCase(ArtifactPermission.Edit)]
        public void InstallProviderAsyncShouldDenyAccessWhenMissingSourceProviderPermission(ArtifactPermission missingPermission)
        {
            // arrange
            AddAllRequiredPermissionForInstallProviderAsync();
            SetupSourceProviderPermission(missingPermission, false);

            SetupRipProviderInstallerSuccessfullResponse();

            InstallProviderRequest request = GetInstallProviderRequest();

            // act
            Func<Task> intallProviderAction = async () => await _providerManager.InstallProviderAsync(request).ConfigureAwait(false);

            // assert
            intallProviderAction.ShouldThrow<InsufficientPermissionException>();
        }

        [Test]
        public async Task InstallProviderAsyncShouldReturnSuccessWhenRipProviderInstallerReturnedSuccess()
        {
            // arrange
            AddAllRequiredPermissionForInstallProviderAsync();
            SetupRipProviderInstallerSuccessfullResponse();

            InstallProviderRequest request = GetInstallProviderRequest();

            // act
            InstallProviderResponse result = await _providerManager.InstallProviderAsync(request).ConfigureAwait(false);

            // assert
            result.Success.Should().BeTrue("because providers were installed successfully");
        }

        [Test]
        public async Task InstallProviderAsyncShouldReturnErrorWhenRipProviderInstallerReturnedFailure()
        {
            // arrange
            const string errorMessage = "ERROR!!!";
            AddAllRequiredPermissionForInstallProviderAsync();
            SetupRipProviderInstallerErrorResponse(errorMessage);

            InstallProviderRequest request = GetInstallProviderRequest();

            // act
            InstallProviderResponse result = await _providerManager.InstallProviderAsync(request).ConfigureAwait(false);

            // assert
            result.Success.Should().BeFalse("because providers were not installed successfully");
            result.ErrorMessage.Should().Be(errorMessage, $"because this value was returned by {nameof(RipProviderInstaller)}");
        }

        [Test]
        public async Task InstallProviderAsyncShouldReturnErrorAndLogFatalWhenRipProviderInstallerReturnedBottom()
        {
            // arrange
            const string expectedErrorMessage = "Unexpected error occured in InstallProviderAsync";
            AddAllRequiredPermissionForInstallProviderAsync();
            SetupRipProviderInstaller(Either<string, Unit>.Bottom);

            InstallProviderRequest request = GetInstallProviderRequest();

            // act
            InstallProviderResponse result = await _providerManager.InstallProviderAsync(request).ConfigureAwait(false);

            // assert
            result.Success.Should().BeFalse("because providers were not installed successfully");
            result.ErrorMessage.Should().Be(expectedErrorMessage, $"because {nameof(RipProviderInstaller)} returned bottom");
            _logger.Received().LogFatal("Unexpected state of Either");
        }

        [Test]
        public void UninstallProviderAsyncShouldDenyAccessWhenMissingWorkspacePermission()
        {
            // arrange
            AddAllRequiredPermissionForUninstallProviderAsync();
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(false);

            SetupRipProviderUninstallerSuccessfullResponse();

            UninstallProviderRequest request = GetUninstallProviderRequest();

            // act
            Func<Task> uninstallProviderAction = async () => await _providerManager.UninstallProviderAsync(request).ConfigureAwait(false);

            // assert
            uninstallProviderAction.ShouldThrow<InsufficientPermissionException>();
        }

        [TestCase(ObjectTypeGuids.IntegrationPoint, ArtifactPermission.Edit)]
        [TestCase(ObjectTypeGuids.IntegrationPoint, ArtifactPermission.Delete)]
        [TestCase(ObjectTypeGuids.SourceProvider, ArtifactPermission.Delete)]
        public void UninstallProviderAsyncShouldDenyAccessWhenMissingArtifactPermission(string objectTypeGuidAsString, ArtifactPermission missingPermission)
        {
            // arrange
            AddAllRequiredPermissionForUninstallProviderAsync();
            Guid objectTypeGuid = new Guid(objectTypeGuidAsString);
            SetupPermission(objectTypeGuid, missingPermission, false);

            SetupRipProviderUninstallerSuccessfullResponse();

            UninstallProviderRequest request = GetUninstallProviderRequest();

            // act
            Func<Task> uninstallProviderAction = async () => await _providerManager.UninstallProviderAsync(request).ConfigureAwait(false);

            // assert
            uninstallProviderAction.ShouldThrow<InsufficientPermissionException>();
        }

        [Test]
        public async Task UninstallProviderAsyncShouldReturnSuccessWhenRipProviderUninstallerReturnedSuccess()
        {
            // arrange
            AddAllRequiredPermissionForUninstallProviderAsync();
            SetupRipProviderUninstallerSuccessfullResponse();

            UninstallProviderRequest request = GetUninstallProviderRequest();

            // act
            UninstallProviderResponse result = await _providerManager.UninstallProviderAsync(request).ConfigureAwait(false);

            // assert
            result.Success.Should().BeTrue("because provider was uninstalled successfully");
        }

        [Test]
        public async Task UninstallProviderAsyncShouldReturnErrorWhenRipProviderUninstallerReturnedError()
        {
            // arrange
            const string errorMessage = "Invalid operation";
            AddAllRequiredPermissionForUninstallProviderAsync();
            SetupRipProviderUninstallerErrorResponse(errorMessage);

            UninstallProviderRequest request = GetUninstallProviderRequest();

            // act
            UninstallProviderResponse result = await _providerManager.UninstallProviderAsync(request).ConfigureAwait(false);

            // assert
            result.Success.Should().BeFalse("because provider was not uninstalled successfully");
            result.ErrorMessage.Should().Be(errorMessage, $"becuase {nameof(RipProviderUninstaller)} returned this error");
        }

        [Test]
        public async Task UninstallProviderAsyncShouldReturnErrorAndLogFatalWhenRipProviderUninstallerReturnedBottom()
        {
            // arrange
            const string expectedErrorMessage = "Unexpected error occured in UninstallProviderAsync";
            AddAllRequiredPermissionForUninstallProviderAsync();
            SetupRipProviderUninstaller(Either<string, Unit>.Bottom);

            UninstallProviderRequest request = GetUninstallProviderRequest();

            // act
            UninstallProviderResponse result = await _providerManager.UninstallProviderAsync(request).ConfigureAwait(false);

            // assert
            result.Success.Should().BeFalse("because providers were not uninstalled successfully");
            result.ErrorMessage.Should().Be(expectedErrorMessage, $"because {nameof(RipProviderUninstaller)} returned bottom");
            _logger.Received().LogFatal("Unexpected state of Either");
        }

        private InstallProviderRequest GetInstallProviderRequest()
        {
            return new InstallProviderRequest
            {
                WorkspaceID = _WORKSPACE_ID,
                ProvidersToInstall = new List<InstallProviderDto> { new InstallProviderDto() }
            };
        }

        private UninstallProviderRequest GetUninstallProviderRequest()
        {
            return new UninstallProviderRequest
            {
                WorkspaceID = _WORKSPACE_ID
            };
        }

        private void AddAllRequiredPermissionForInstallProviderAsync()
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);

            SetupSourceProviderPermission(ArtifactPermission.Create, true);
            SetupSourceProviderPermission(ArtifactPermission.Edit, true);
        }

        private void AddAllRequiredPermissionForUninstallProviderAsync()
        {
            _permissionRepository.UserHasPermissionToAccessWorkspace().Returns(true);

            SetupSourceProviderPermission(ArtifactPermission.Delete, true);

            SetupIntegrationPointPermission(ArtifactPermission.Edit, true);
            SetupIntegrationPointPermission(ArtifactPermission.Delete, true);
        }

        private void SetupSourceProviderPermission(ArtifactPermission artifactPermission, bool hasPermission)
        {
            Guid sourceProviderObjectTypeGuid = Guid.Parse(ObjectTypeGuids.SourceProvider);
            SetupPermission(sourceProviderObjectTypeGuid, artifactPermission, hasPermission);

        }

        private void SetupIntegrationPointPermission(ArtifactPermission artifactPermission, bool hasPermission)
        {
            Guid integrationPointGuid = Guid.Parse(ObjectTypeGuids.IntegrationPoint);
            SetupPermission(integrationPointGuid, artifactPermission, hasPermission);
        }

        private void SetupPermission(Guid objectTypeGuid, ArtifactPermission artifactPermission, bool hasPermission)
        {
            _permissionRepository
                .UserHasArtifactTypePermission(objectTypeGuid, artifactPermission)
                .Returns(hasPermission);
        }

        private void SetupRipProviderInstallerSuccessfullResponse()
        {
            SetupRipProviderInstaller(Right<string, Unit>(Unit.Default));
        }

        private void SetupRipProviderInstallerErrorResponse(string error)
        {
            SetupRipProviderInstaller(Left<string, Unit>(error));
        }

        private void SetupRipProviderInstaller(Either<string, Unit> resultToReturn)
        {
            var ripProviderInstallerMock = new Mock<IRipProviderInstaller>();
            ripProviderInstallerMock
                .Setup(x => x.InstallProvidersAsync(It.IsAny<IEnumerable<SourceProvider>>()))
                .Returns(Task.FromResult(resultToReturn));
            _container
                .Resolve<IRipProviderInstaller>()
                .Returns(ripProviderInstallerMock.Object);
        }

        private void SetupRipProviderUninstallerSuccessfullResponse()
        {
            SetupRipProviderUninstaller(Right<string, Unit>(Unit.Default));
        }

        private void SetupRipProviderUninstallerErrorResponse(string error)
        {
            SetupRipProviderUninstaller(Left<string, Unit>(error));
        }

        private void SetupRipProviderUninstaller(Either<string, Unit> resultToReturn)
        {
            var ripProviderInstallerMock = new Mock<IRipProviderUninstaller>();
            ripProviderInstallerMock
                .Setup(x => x.UninstallProvidersAsync(It.IsAny<int>()))
                .Returns(Task.FromResult(resultToReturn));
            _container
                .Resolve<IRipProviderUninstaller>()
                .Returns(ripProviderInstallerMock.Object);
        }
    }
}
