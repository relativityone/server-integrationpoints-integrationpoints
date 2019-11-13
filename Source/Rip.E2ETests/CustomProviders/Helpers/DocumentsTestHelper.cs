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
			string joinedIdentifiers = JoinQuotedIdentifiers(identifiers);

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

			return documents.ToDictionary(
				document => document.FieldValues.GetTextFieldValue("Control Number"),
				document => document.FieldValues.GetTextFieldValue("Extracted Text")
			);
		}

		public static async Task<Dictionary<string, string>> GetSampleTextForJsonObjectsAsync(
			IRelativityObjectManager objectManager,
			int targetObjectTypeArtifactID,
			IEnumerable<string> identifiers)
		{
			string joinedIdentifiers = JoinQuotedIdentifiers(identifiers);

			var queryRequest = new QueryRequest
			{
				ObjectType = new ObjectTypeRef
				{
					ArtifactTypeID = targetObjectTypeArtifactID
				},
				Condition = $"'Name' IN [{joinedIdentifiers}]",
				Fields = new[]
				{
					new FieldRef
					{
						Name = "Name"
					},
					new FieldRef
					{
						Name = "Sample Text Field"
					}
				}
			};

			List<RelativityObject> documents = await objectManager
				.QueryAsync(queryRequest)
				.ConfigureAwait(false);

			return documents.ToDictionary(
				document => document.FieldValues.GetTextFieldValue("Name"),
				document => document.FieldValues.GetTextFieldValue("Sample Text Field")
			);
		}

		private static string JoinQuotedIdentifiers(IEnumerable<string> identifiers)
		{
			IEnumerable<string> quotedIdentifiers = identifiers.Select(identifier => $"'{identifier}'");
			return string.Join(",", quotedIdentifiers);
		}
	}
}