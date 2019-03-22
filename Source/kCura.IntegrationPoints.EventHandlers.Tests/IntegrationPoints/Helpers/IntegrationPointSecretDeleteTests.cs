using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.IntegrationPoints.Data.SecretStore;
using kCura.IntegrationPoints.EventHandlers.IntegrationPoints.Helpers.Implementations;
using NSubstitute;
using NUnit.Framework;
using Relativity.SecretCatalog;

namespace kCura.IntegrationPoints.EventHandlers.Tests.IntegrationPoints.Helpers
{
	[TestFixture]
	public class IntegrationPointSecretDeleteTests : TestBase
	{
		private const int _INTEGRATION_POINT_ID = 659252;

		private IntegrationPointSecretDelete _integrationPointSecretDelete;
		private ISecretCatalog _secretCatalog;
		private ISecretManager _secretManager;
		private IIntegrationPointRepository _integrationPointRepository;

		public override void SetUp()
		{
			_secretManager = Substitute.For<ISecretManager>();
			_secretCatalog = Substitute.For<ISecretCatalog>();
			_integrationPointRepository = Substitute.For<IIntegrationPointRepository>();
			_integrationPointSecretDelete = new IntegrationPointSecretDelete(_secretManager, _secretCatalog, _integrationPointRepository);
		}

		[Test]
		[TestCase("")]
		[TestCase(" ")]
		[TestCase(null)]
		public void ItShouldSkipForEmptySecret(string secret)
		{
			_integrationPointRepository.GetSecuredConfiguration(_INTEGRATION_POINT_ID).Returns(secret);

			_integrationPointSecretDelete.DeleteSecret(_INTEGRATION_POINT_ID);

			_secretCatalog.Received(0).RevokeSecret(Arg.Any<SecretRef>());
		}

		[Test]
		public void ItShouldRetrieveSecretIdentifier()
		{
			string securedConfiguration = "expected_secured_configuration";

			_integrationPointRepository.GetSecuredConfiguration(_INTEGRATION_POINT_ID).Returns(securedConfiguration);

			_integrationPointSecretDelete.DeleteSecret(_INTEGRATION_POINT_ID);

			_secretManager.Received(1).RetrieveIdentifier(securedConfiguration);
		}

		[Test]
		public void ItShouldRevokeSecret()
		{
			var expectedSecretRef = new SecretRef();

			_integrationPointRepository.GetSecuredConfiguration(_INTEGRATION_POINT_ID).Returns("secured_configuration");

			_secretManager.RetrieveIdentifier(Arg.Any<string>()).Returns(expectedSecretRef);

			_integrationPointSecretDelete.DeleteSecret(_INTEGRATION_POINT_ID);

			_secretCatalog.Received(1).RevokeSecret(expectedSecretRef);
		}
	}
}