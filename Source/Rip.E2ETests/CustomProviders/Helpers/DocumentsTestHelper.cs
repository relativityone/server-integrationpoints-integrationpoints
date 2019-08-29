using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using kCura.IntegrationPoints.Data;
using kCura.IntegrationPoints.Data.Repositories;
using Relativity.Services.Objects.DataContracts;

namespace Rip.E2ETests.CustomProviders.Helpers
{
	internal static class DocumentsTestHelper
	{
		public static async Task<Dictionary<string, string>> GetExtractedTextForDocumentsAsync(
			IRelativityObjectManager objectManager,
			IEnumerable<string> identifiers)
		{
			IEnumerable<string> escapedIdentifiers = identifiers.Select(identifier => $"'{identifier}'");
			string joinedIdentifiers = string.Join(",", escapedIdentifiers);

			var queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					Guid = ObjectTypeGuids.DocumentGuid
				},
				Condition = $"'Control Number' IN [{joinedIdentifiers}]",
				Fields = new[]
				{
					new FieldRef
					{
						Name = "Control Number"
					},
					new FieldRef
					{
						Name = "Extracted Text"
					}
				}
			};

			List<RelativityObject> documents = await objectManager
				.QueryAsync(queryRequest)
				.ConfigureAwait(false);

			return documents
				.Select(document => new
				{
					ControlNumber = document.FieldValues.GetTextFieldValue("Control Number"),
					ExtractedText = document.FieldValues.GetTextFieldValue("Extracted Text")
				})
				.ToDictionary(x => x.ControlNumber, x => x.ExtractedText);
		}
	}
}
