using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data.Repositories;
using kCura.Relativity.Client;
using Relativity.Services.Objects.DataContracts;

namespace Rip.E2ETests.CustomProviders.Helpers
{
	internal static class FieldsTestHelper
	{
		public static async Task<Dictionary<string, int>> GetIdentifiersForFieldsAsync(
			IRelativityObjectManager objectManager, 
			int targetRdoArtifactID, 
			IEnumerable<string> fieldNames)
		{
			IEnumerable<string> escapedFieldNames = fieldNames.Select(x => $"'{x}'");
			var fieldNamesList = string.Join(",", escapedFieldNames);

			var queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef { ArtifactTypeID = (int)ArtifactType.Field },
				Fields = new[]
				{
					new FieldRef
					{
						Name = "Name"
					}
				},
				Condition = $"'Object Type Artifact Type Id' == OBJECT {targetRdoArtifactID} AND 'Name' IN [{fieldNamesList}]"
			};
			List<RelativityObject> fields = await objectManager.QueryAsync(queryRequest).ConfigureAwait(false);

			Dictionary<string, int> fieldNamesToArtifactIDMapping = fields
				.Select(field => new
				{
					Name = field.FieldValues.GetTextFieldValue("Name"),
					field.ArtifactID
				})
				.ToDictionary(x => x.Name, x => x.ArtifactID);
			return fieldNamesToArtifactIDMapping;
		}
	}
}
