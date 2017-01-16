using System.Collections.Generic;
using System.Data;
using System.Linq;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoint.Tests.Core.TestHelpers;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.ProviderManager
{
	[TestFixture]
	public class ProviderManagerTests : SourceProviderTemplate
	{
		public ProviderManagerTests() : base($"KeplerService_{Utils.FormatedDateTimeNow}")
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

		[Test]
		public void ItShouldRetrieveSourceProvidersGuids()
		{
			IDictionary<string, int> providers = GetAllProvidersByGuids("SourceProvider");

			foreach (var providerGuid in providers.Keys)
			{
				var sourceProviderId = _client.GetSourceProviderArtifactIdAsync(WorkspaceArtifactId, providerGuid).Result;
				Assert.That(sourceProviderId, Is.EqualTo(providers[providerGuid]));
			}
		}

		[Test]
		public void ItShouldRetrieveDestinationProvidersGuids()
		{
			IDictionary<string, int> providers = GetAllProvidersByGuids("DestinationProvider");

			foreach (var providerGuid in providers.Keys)
			{
				var destinationProviderId = _client.GetDestinationProviderArtifactIdAsync(WorkspaceArtifactId, providerGuid).Result;
				Assert.That(destinationProviderId, Is.EqualTo(providers[providerGuid]));
			}
		}

		[Test]
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

		[Test]
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