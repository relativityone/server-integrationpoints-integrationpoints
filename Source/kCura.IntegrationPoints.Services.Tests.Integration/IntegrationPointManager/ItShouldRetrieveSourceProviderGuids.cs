using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.IntegrationPointManager
{
	[TestFixture]
	public class ItShouldRetrieveSourceProviderGuids : SourceProviderTemplate
	{
		public ItShouldRetrieveSourceProviderGuids() : base($"KeplerService_{Utils.Identifier}")
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
			IDictionary<string, int> sourceProviders = GetAllSourceProviders();

			foreach (var sourceProviderGuid in sourceProviders.Keys)
			{
				var sourceProviderId = _client.GetSourceProviderArtifactIdAsync(WorkspaceArtifactId, sourceProviderGuid).Result;
				Assert.That(sourceProviderId, Is.EqualTo(sourceProviders[sourceProviderGuid]));
			}
		}

		private IDictionary<string, int> GetAllSourceProviders()
		{
			string sqlStatement = "SELECT [ArtifactID], [Identifier] FROM [SourceProvider]";
			var sourceProvidersDataTable = Helper.GetDBContext(WorkspaceArtifactId).ExecuteSqlStatementAsDataTable(sqlStatement);

			IDictionary<string, int> sourceProviders = new Dictionary<string, int>();
			foreach (DataRow dataRow in sourceProvidersDataTable.Rows)
			{
				sourceProviders.Add(dataRow["Identifier"].ToString(), int.Parse(dataRow["ArtifactID"].ToString()));
			}
			return sourceProviders;
		}
	}
}