using System.Web;

namespace kCura.IntegrationPoints.Web.Models
{
	using System;
	using System.Collections.Generic;
	using System.Text;
	using kCura.IntegrationPoints.Data.Queries;
	using kCura.Relativity.Client;

	public class SavedSearchModel
	{
		private SavedSearchModel()
		{
		}

		public int Value { get; private set; }
		public String DisplayName { get; private set; }

		public static IList<SavedSearchModel> GetAllPublicSavedSearches(IRSAPIClient context)
		{
			const String identifier = "Text Identifier";
			const String owner = "Owner";

			GetSavedSearchesQuery query = new GetSavedSearchesQuery(context);
			List<Artifact> artifacts = query.ExecuteQuery().QueryArtifacts;
			List<SavedSearchModel> result = new List<SavedSearchModel>(artifacts.Count);
			foreach (var artifact in artifacts)
			{
				// if the search doesn't have an Owner, then the search is public
				Field ownerField = artifact.getFieldByName(owner);
				byte[] fieldValue = ownerField.Value as byte[];
				if (fieldValue != null && fieldValue.Length > 0)
				{
					continue;
				}

				Field textIdentifierField = artifact.getFieldByName(identifier);
				if (textIdentifierField != null && textIdentifierField.Value != null)
				{
					String searchName =	Encoding.Unicode.GetString((byte[])artifact.getFieldByName(identifier).Value);
					result.Add(new SavedSearchModel() { DisplayName = searchName, Value = artifact.ArtifactID });
				}
			}
			return result;
		}
	}
}