using System.Collections.Generic;
using System.Data;
using Relativity.API;

namespace kCura.IntegrationPoints.Services.Tests.Integration.Helpers
{
	public class ChoicesHelper
	{
		public static IDictionary<string, int> GetAllChoiceUsingFieldGuid(string guid, int workspaceArtifactId, IHelper helper)
		{
			string sqlStatement = string.Format(_SELECT_CHOICES_ON_FIELD_WITH_GUID, guid);
			var dataTable = helper.GetDBContext(workspaceArtifactId).ExecuteSqlStatementAsDataTable(sqlStatement);

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