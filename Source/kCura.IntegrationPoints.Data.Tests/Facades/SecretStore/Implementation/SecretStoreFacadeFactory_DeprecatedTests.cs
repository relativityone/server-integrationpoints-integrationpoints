using FluentAssertions;
using kCura.IntegrationPoints.Data.Facades.SecretStore;
using kCura.IntegrationPoints.Data.Facades.SecretStore.Implementation;
using Moq;
using NUnit.Framework;
using Relativity.API;

namespace kCura.IntegrationPoints.Data.Tests.Facades.SecretStore.Implementation
{
    [TestFixture, Category("Unit")]
    public class SecretStoreFacadeFactory_DeprecatedTests
    {
        private Mock<IAPILog> _loggerMock;
        private Mock<ISecretStore> _secretStoreMock;

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            _loggerMock = new Mock<IAPILog>();
            _secretStoreMock = new Mock<ISecretStore>();
        }

        [Test]
        public void ShouldCreateFacadeOfTypeSecretStoreFacadeRetryDecorator()
        {
            // act
            ISecretStoreFacade createdInstance = SecretStoreFacadeFactory_Deprecated.Create(
                () => _secretStoreMock.Object,
                _loggerMock.Object
            );

            // assert
            createdInstance.Should().BeOfType<SecretStoreFacadeRetryDecorator>();
        }
    }
}
