using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace Relativity.IntegrationPoints.Services.Tests.Integration.IntegrationPointManager
{
	[TestFixture]
	[Feature.DataTransfer.IntegrationPoints]
	public class ItShouldRetrieveIntegrationPointArtifactTypeId : SourceProviderTemplate
	{
		public ItShouldRetrieveIntegrationPointArtifactTypeId() : base($"KeplerService_{Utils.FormattedDateTimeNow}")
		{
		}

		private IIntegrationPointManager _client;

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			_client = Helper.CreateProxy<IIntegrationPointManager>();
		}

		public override void SuiteTeardown()
		{
			base.SuiteTeardown();
			_client?.Dispose();
		}

		[IdentifiedTest("d8813b03-466b-486b-902f-6301c89e74b8")]
		public void Execute()
		{
			string sqlStatement = "SELECT [ArtifactTypeID] FROM [ArtifactType] WHERE [ArtifactType] LIKE 'IntegrationPoint'";

			var expectedArtifactTypeId = Helper.GetDBContext(WorkspaceArtifactId).ExecuteSqlStatementAsScalar<int>(sqlStatement);

			var actualArtifactTypeId = _client.GetIntegrationPointArtifactTypeIdAsync(WorkspaceArtifactId).Result;

			Assert.That(actualArtifactTypeId, Is.EqualTo(expectedArtifactTypeId));
		}
	}
}