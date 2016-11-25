using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.IntegrationPointManager
{
	internal class ItShouldRetrieveIntegrationPointArtifactTypeId : SourceProviderTemplate
	{
		public ItShouldRetrieveIntegrationPointArtifactTypeId() : base($"KeplerService_{Utils.Identifier}")
		{
		}

		private IIntegrationPointManager _client;

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			_client = Helper.CreateAdminProxy<IIntegrationPointManager>();
		}

		public override void SuiteTeardown()
		{
			base.SuiteTeardown();
			_client?.Dispose();
		}

		[Test]
		public void Execute()
		{
			string sqlStatement = "SELECT [ArtifactTypeID] FROM [ArtifactType] WHERE [ArtifactType] LIKE 'IntegrationPoint'";

			var expectedArtifactTypeId = Helper.GetDBContext(WorkspaceArtifactId).ExecuteSqlStatementAsScalar<int>(sqlStatement);

			var actualArtifactTypeId = _client.GetIntegrationPointArtifactTypeIdAsync(WorkspaceArtifactId).Result;

			Assert.That(actualArtifactTypeId, Is.EqualTo(expectedArtifactTypeId));
		}
	}
}