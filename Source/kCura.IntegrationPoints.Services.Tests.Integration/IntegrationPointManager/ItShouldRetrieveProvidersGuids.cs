using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.IntegrationPointManager
{
	[TestFixture]
	public class ItShouldRetrieveProvidersGuids : SourceProviderTemplate
	{
		public ItShouldRetrieveProvidersGuids() : base($"KeplerService_{Utils.FormatedDateTimeNow}")
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
		public void ItShouldRetrieveSourceProvidersGuids()
		{
			IDictionary<string, int> sourceProviders = GetAllProviders("SourceProvider");

			foreach (var sourceProviderGuid in sourceProviders.Keys)
			{
				var sourceProviderId = _client.GetSourceProviderArtifactIdAsync(WorkspaceArtifactId, sourceProviderGuid).Result;
				Assert.That(sourceProviderId, Is.EqualTo(sourceProviders[sourceProviderGuid]));
			}
		}

		[Test]
		public void ItShouldRetrieveDestinationProvidersGuids()
		{
			IDictionary<string, int> destinationProviders = GetAllProviders("DestinationProvider");

			foreach (var destinationProviderGuid in destinationProviders.Keys)
			{
				var destinationProviderId = _client.GetDestinationProviderArtifactIdAsync(WorkspaceArtifactId, destinationProviderGuid).Result;
				Assert.That(destinationProviderId, Is.EqualTo(destinationProviders[destinationProviderGuid]));
			}
		}

		private IDictionary<string, int> GetAllProviders(string tableName)
		{
			string sqlStatement = $"SELECT [ArtifactID], [Identifier] FROM [{tableName}]";
			var providersDataTable = Helper.GetDBContext(WorkspaceArtifactId).ExecuteSqlStatementAsDataTable(sqlStatement);

			IDictionary<string, int> providers = new Dictionary<string, int>();
			foreach (DataRow dataRow in providersDataTable.Rows)
			{
				providers.Add(dataRow["Identifier"].ToString(), int.Parse(dataRow["ArtifactID"].ToString()));
			}
			return providers;
		}
	}
}