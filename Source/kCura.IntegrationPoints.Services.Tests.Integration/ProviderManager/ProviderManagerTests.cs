using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using NUnit.Framework;
using Relativity.Testing.Identification;

namespace kCura.IntegrationPoints.Services.Tests.Integration.ProviderManager
{
	[TestFixture]
	public class ProviderManagerTests : SourceProviderTemplate
	{
		public ProviderManagerTests() : base($"KeplerService_{Utils.FormattedDateTimeNow}")
		{
		}

		private IProviderManager _client;

		public override void SuiteSetup()
		{
			base.SuiteSetup();
			_client = Helper.CreateAdminProxy<IProviderManager>();
		}

		public override void SuiteTeardown()
		{
			base.SuiteTeardown();
			_client?.Dispose();
		}

		[IdentifiedTest("1dc24aa4-b2d2-4def-9f7d-eea0adcfe686")]
		public void ItShouldRetrieveSourceProvidersGuids()
		{
			IDictionary<string, int> providers = GetAllProvidersByGuids("SourceProvider");

			foreach (var providerGuid in providers.Keys)
			{
				var sourceProviderId = _client.GetSourceProviderArtifactIdAsync(WorkspaceArtifactId, providerGuid).Result;
				Assert.That(sourceProviderId, Is.EqualTo(providers[providerGuid]));
			}
		}

		[IdentifiedTest("2ee21137-6a20-49a9-a7d7-ed380a70e91d")]
		public void ItShouldRetrieveDestinationProvidersGuids()
		{
			IDictionary<string, int> providers = GetAllProvidersByGuids("DestinationProvider");

			foreach (var providerGuid in providers.Keys)
			{
				var destinationProviderId = _client.GetDestinationProviderArtifactIdAsync(WorkspaceArtifactId, providerGuid).Result;
				Assert.That(destinationProviderId, Is.EqualTo(providers[providerGuid]));
			}
		}

		[IdentifiedTest("aef37eb9-f1aa-408a-8d1b-9baf28ab6c67")]
		public void ItShouldRetrieveAllSourceProviders()
		{
			IDictionary<string, int> providers = GetAllProvidersByName("SourceProvider");

			IList<ProviderModel> providerModels = _client.GetSourceProviders(WorkspaceArtifactId).Result;

			Assert.That(providerModels.Count, Is.EqualTo(providers.Keys.Count));

			foreach (var providerName in providers.Keys)
			{
				var providerModel = providerModels.FirstOrDefault(x => x.Name == providerName);

				Assert.That(providerModel, Is.Not.Null);
				Assert.That(providerModel.ArtifactId, Is.EqualTo(providers[providerName]));
			}
		}

		[IdentifiedTest("30d039e8-aad9-481f-b64c-a50b260675e3")]
		public void ItShouldRetrieveAllDestinationProviders()
		{
			IDictionary<string, int> providers = GetAllProvidersByName("DestinationProvider");

			IList<ProviderModel> providerModels = _client.GetDestinationProviders(WorkspaceArtifactId).Result;

			Assert.That(providerModels.Count, Is.EqualTo(providers.Keys.Count));

			foreach (var providerName in providers.Keys)
			{
				var providerModel = providerModels.FirstOrDefault(x => x.Name == providerName);

				Assert.That(providerModel, Is.Not.Null);
				Assert.That(providerModel.ArtifactId, Is.EqualTo(providers[providerName]));
			}
		}

		private IDictionary<string, int> GetAllProvidersByGuids(string tableName)
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

		private IDictionary<string, int> GetAllProvidersByName(string tableName)
		{
			string sqlStatement = $"SELECT [ArtifactID], [Name] FROM [{tableName}]";
			var providersDataTable = Helper.GetDBContext(WorkspaceArtifactId).ExecuteSqlStatementAsDataTable(sqlStatement);

			IDictionary<string, int> providers = new Dictionary<string, int>();
			foreach (DataRow dataRow in providersDataTable.Rows)
			{
				providers.Add(dataRow["Name"].ToString(), int.Parse(dataRow["ArtifactID"].ToString()));
			}
			return providers;
		}
	}
}