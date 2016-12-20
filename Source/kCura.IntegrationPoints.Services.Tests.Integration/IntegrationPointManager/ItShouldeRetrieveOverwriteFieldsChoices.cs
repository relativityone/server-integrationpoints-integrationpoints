using System;
using System.Collections.Generic;
using System.Data;
using kCura.IntegrationPoint.Tests.Core.Templates;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Services.Interfaces.Private.Models;
using kCura.IntegrationPoints.Services.Tests.Integration.Helpers;
using NUnit.Framework;

namespace kCura.IntegrationPoints.Services.Tests.Integration.IntegrationPointManager
{
	public class ItShouldeRetrieveOverwriteFieldsChoices : SourceProviderTemplate
	{
		public ItShouldeRetrieveOverwriteFieldsChoices() : base($"choices_{Utils.FormatedDateTimeNow}")
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
			_client.Dispose();
		}

		[Test]
		public void Execute()
		{
			var expectedChoices = GetAllChoiceUsingFieldGuid(IntegrationPointFieldGuids.OverwriteFields);

			var overwriteFieldModels = _client.GetOverwriteFieldsChoicesAsync(WorkspaceArtifactId).Result;

			Assert.That(overwriteFieldModels,
				Is.EquivalentTo(expectedChoices.Keys).Using(new Func<OverwriteFieldsModel, string, bool>((x, y) => (x.Name == y) && (x.ArtifactId == expectedChoices[y]))));
		}

		private IDictionary<string, int> GetAllChoiceUsingFieldGuid(string guid)
		{
			string sqlStatement = string.Format(_SELECT_CHOICES_ON_FIELD_WITH_GUID, guid);
			var dataTable = Helper.GetDBContext(WorkspaceArtifactId).ExecuteSqlStatementAsDataTable(sqlStatement);

			IDictionary<string, int> choices = new Dictionary<string, int>();
			foreach (DataRow dataRow in dataTable.Rows)
			{
				choices.Add(dataRow["Name"].ToString(), int.Parse(dataRow["ArtifactID"].ToString()));
			}
			return choices;
		}

		private const string _SELECT_CHOICES_ON_FIELD_WITH_GUID =
			@"SELECT C.ArtifactID, C.Name FROM Code C
			JOIN CodeType CT ON C.CodeTypeID = CT.CodeTypeID
			JOIN Field F ON F.CodeTypeID = CT.CodeTypeID
			JOIN ArtifactGuid AG ON AG.ArtifactID = F.ArtifactID
			WHERE AG.ArtifactGuid LIKE '{0}'";
	}
}