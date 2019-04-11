using FluentAssertions;
using kCura.EventHandler;
using kCura.IntegrationPoints.Services;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System.Threading.Tasks;

namespace kCura.IntegrationPoints.SourceProviderInstaller.Tests
{
    [TestFixture]
    public class IntegrationPointSourceProviderUninstallerTests
    {
        private Mock<IProviderManager> _providerManagerMock;

        private SubjectUnderTests _sut;

        private const int _APPLICATION_ID = 3232;
        private const int _WORKSPACE_ID = 84221;

        [SetUp]
        public void SetUp()
        {
            _providerManagerMock = new Mock<IProviderManager>();

            var servicesManagerMock = new Mock<IServicesMgr>();
            servicesManagerMock
                .Setup(x => x.CreateProxy<IProviderManager>(It.IsAny<ExecutionIdentity>()))
                .Returns(_providerManagerMock.Object);

            var helperMock = new Mock<IEHHelper>
            {
                DefaultValue = DefaultValue.Mock
            };
            helperMock
                .Setup(x => x.GetServicesManager())
                .Returns(servicesManagerMock.Object);
            helperMock
                .Setup(x => x.GetActiveCaseID())
                .Returns(_WORKSPACE_ID);

            _sut = new SubjectUnderTests(helperMock.Object);
        }

        [Test]
        public void ShouldSendUninstallRequest()
        {
            // arrange
            var response = new UninstallProviderResponse();
            _providerManagerMock
                .Setup(x => x.UninstallProviderAsync(It.IsAny<UninstallProviderRequest>()))
                .Returns(Task.FromResult(response));

            // act
            _sut.Execute();

            // assert
            _providerManagerMock.Verify(x =>
                x.UninstallProviderAsync(
                    It.Is<UninstallProviderRequest>(request => ValidateUninstallRequestIsValid(request))
                )
            );
        }

        [Test]
        public void ShouldReturnSuccessWhenKeplerReturnedSuccess()
        {
            var response = new UninstallProviderResponse();
            _providerManagerMock
                .Setup(x => x.UninstallProviderAsync(It.IsAny<UninstallProviderRequest>()))
                .Returns(Task.FromResult(response));

            // act
            Response result = _sut.Execute();

            // assert
            result.Success.Should().BeTrue("because kepler returned success response");
        }

        [Test]
        public void ShouldReturnErrorWhenKeplerReturnedError()
        {
            string errorMessage = "error in kepler";
            var response = new UninstallProviderResponse(errorMessage);
            _providerManagerMock
                .Setup(x => x.UninstallProviderAsync(It.IsAny<UninstallProviderRequest>()))
                .Returns(Task.FromResult(response));

            // act
            Response result = _sut.Execute();

            // assert
            result.Success.Should().BeFalse("because kepler returned error response");
            result.Message.Should().Be(errorMessage);
        }

        private bool ValidateUninstallRequestIsValid(UninstallProviderRequest request)
        {
            request.ApplicationID.Should().Be(_APPLICATION_ID);
            request.WorkspaceID.Should().Be(_WORKSPACE_ID);

            return true;
        }

        private class SubjectUnderTests : IntegrationPointSourceProviderUninstaller
        {
            public SubjectUnderTests(IEHHelper helper)
            {
                Helper = helper;
                ApplicationArtifactId = _APPLICATION_ID;
            }
        }
    }
}
