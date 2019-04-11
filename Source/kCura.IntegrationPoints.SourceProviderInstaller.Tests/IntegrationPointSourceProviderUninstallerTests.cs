using kCura.IntegrationPoints.Services;
using Moq;
using NUnit.Framework;
using Relativity.API;
using System.Threading.Tasks;
using FluentAssertions;
using kCura.EventHandler;

namespace kCura.IntegrationPoints.SourceProviderInstaller.Tests
{
    [TestFixture]
    public class IntegrationPointSourceProviderUninstallerTests
    {
        private Mock<IProviderManager> _providerManager;

        private SubjectUnderTests _sut;

        private const int _APPLICATION_ID = 3232;
        private const int _WORKSPACE_ID = 84221;

        [SetUp]
        public void SetUp()
        {
            _providerManager = new Mock<IProviderManager>();

            var servicesManager = new Mock<IServicesMgr>();
            servicesManager
                .Setup(x => x.CreateProxy<IProviderManager>(It.IsAny<ExecutionIdentity>()))
                .Returns(_providerManager.Object);

            var helper = new Mock<IEHHelper>
            {
                DefaultValue = DefaultValue.Mock
            };
            helper
                .Setup(x => x.GetServicesManager())
                .Returns(servicesManager.Object);
            helper
                .Setup(x => x.GetActiveCaseID())
                .Returns(_WORKSPACE_ID);

            _sut = new SubjectUnderTests(helper.Object);
        }

        [Test]
        public void ShouldSendUninstallRequest()
        {
            // arrange
            var response = new UninstallProviderResponse();
            _providerManager
                .Setup(x => x.UninstallProviderAsync(It.IsAny<UninstallProviderRequest>()))
                .Returns(Task.FromResult(response));

            // act
            _sut.Execute();

            // assert
            _providerManager.Verify(x =>
                x.UninstallProviderAsync(
                    It.Is<UninstallProviderRequest>(request => ValidateUninstallRequestIsValid(request))
                )
            );
        }

        [Test]
        public void ShouldReturnSuccessWhenKeplerReturnedSuccess()
        {
            var response = new UninstallProviderResponse();
            _providerManager
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
            _providerManager
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
            return request.ApplicationID == _APPLICATION_ID && request.WorkspaceID == _WORKSPACE_ID;
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
