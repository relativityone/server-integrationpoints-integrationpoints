using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.Repositories.DTO;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using NSubstitute;
using NUnit.Framework;

namespace kCura.IntegrationPoints.EventHandlers.Tests.IntegrationPoints.Helpers
{
    [TestFixture, Category("Unit")]
    public class IntegrationPointSecretDeleteTests : TestBase
    {
        private IIntegrationPointRepository _integrationPointRepository;
        private IntegrationPointSecretDelete _integrationPointSecretDelete;
        private ISecretsRepository _secretsRepository;

        private const int _INTEGRATION_POINT_ID = 659252;
        private const int _WORKSPACE_ID = 1001;

        public override void SetUp()
        {
            _secretsRepository = Substitute.For<ISecretsRepository>();
            _integrationPointRepository = Substitute.For<IIntegrationPointRepository>();
            _integrationPointSecretDelete = new IntegrationPointSecretDelete(
                _WORKSPACE_ID,
                _secretsRepository, 
                _integrationPointRepository);
        }

        [Test]
        [TestCase("")]
        [TestCase(" ")]
        [TestCase(null)]
        public void ItShouldSkipForEmptySecret(string secret)
        {
            _integrationPointRepository.GetEncryptedSecuredConfiguration(_INTEGRATION_POINT_ID).Returns(secret);

            _integrationPointSecretDelete.DeleteSecret(_INTEGRATION_POINT_ID);

            _secretsRepository.Received(0).DeleteAllRipSecretsFromIntegrationPointAsync(
                Arg.Any<int>(),
                Arg.Any<int>()
            );
        }

        [Test]
        public void ItShouldRetrieveSecretIdentifier()
        {
            string securedConfiguration = "655bb9b7-94fd-4e22-a361-026f970d5bc5";

            _integrationPointRepository
                .GetEncryptedSecuredConfiguration(_INTEGRATION_POINT_ID)
                .Returns(securedConfiguration);

            _integrationPointSecretDelete.DeleteSecret(_INTEGRATION_POINT_ID);
        }

        [Test]
        public void ItShouldRevokeSecret()
        {
            string expectedSecret = "655bb9b7-94fd-4e22-a361-026f970d5bc5";

            _integrationPointRepository
                .GetEncryptedSecuredConfiguration(_INTEGRATION_POINT_ID)
                .Returns(expectedSecret);

            _integrationPointSecretDelete.DeleteSecret(_INTEGRATION_POINT_ID);

            _secretsRepository
                .Received(1)
                .DeleteAllRipSecretsFromIntegrationPointAsync(
                    Arg.Is(_WORKSPACE_ID),
                    Arg.Is(_INTEGRATION_POINT_ID)
                );
        }
    }
}