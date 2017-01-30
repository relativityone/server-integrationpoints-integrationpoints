using kCura.IntegrationPoint.Tests.Core;
using kCura.IntegrationPoints.Data;
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
		private IGenericLibrary<Data.IntegrationPoint> _library;
		private ISecretCatalog _secretCatalog;
		private ISecretManager _secretManager;

		public override void SetUp()
		{
			_secretManager = Substitute.For<ISecretManager>();
			_secretCatalog = Substitute.For<ISecretCatalog>();
			_library = Substitute.For<IGenericLibrary<Data.IntegrationPoint>>();
			_integrationPointSecretDelete = new IntegrationPointSecretDelete(_secretManager, _secretCatalog, _library);
		}

		[Test]
		[TestCase("")]
		[TestCase(" ")]
		[TestCase(null)]
		public void ItShouldSkipForEmptySecret(string secret)
		{
			_library.Read(_INTEGRATION_POINT_ID).Returns(new Data.IntegrationPoint
			{
				SecuredConfiguration = secret
			});

			_integrationPointSecretDelete.DeleteSecret(_INTEGRATION_POINT_ID);

			_secretCatalog.Received(0).RevokeSecret(Arg.Any<SecretRef>());
		}

		[Test]
		public void ItShouldRetrieveSecretIdentifier()
		{
			string securedConfiguration = "expected_secured_configuration";

			_library.Read(_INTEGRATION_POINT_ID).Returns(new Data.IntegrationPoint
			{
				SecuredConfiguration = securedConfiguration
			});

			_integrationPointSecretDelete.DeleteSecret(_INTEGRATION_POINT_ID);

			_secretManager.Received(1).RetrieveIdentifier(securedConfiguration);
		}

		[Test]
		public void ItShouldRevokeSecret()
		{
			var expectedSecretRef = new SecretRef();

			_library.Read(_INTEGRATION_POINT_ID).Returns(new Data.IntegrationPoint
			{
				SecuredConfiguration = "secured_configuration"
			});

			_secretManager.RetrieveIdentifier(Arg.Any<string>()).Returns(expectedSecretRef);

			_integrationPointSecretDelete.DeleteSecret(_INTEGRATION_POINT_ID);

			_secretCatalog.Received(1).RevokeSecret(expectedSecretRef);
		}
	}
}